using Microsoft.Maui.Layouts;
using System.Collections;
using MauiVirtualList.Enums;
using System.Collections.ObjectModel;
using MauiVirtualList.Utils;

namespace MauiVirtualList.Controls;

public class Body : Layout, ILayoutManager
{
    private readonly List<VirtualItem> _cachePool = [];
    private readonly DataTemplate _defaultItemTemplate = new(() => new Label { Text = "NO_TEMPLATE" });
    private double _scrollY;

    #region props
    // item template
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(Body),
        null,
        propertyChanged: (b, o, n) =>
        {
            if (b is Body self)
                self.Update(true, self.ViewPortWidth, self.ViewPortHeight);
        }
    );
    public DataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty) as DataTemplate;
        set => SetValue(ItemTemplateProperty, value);
    }

    public IList? ItemsSource { get; set; }
    public int Cunt => ItemsSource != null ? ItemsSource.Count : 0;
    public double ItemsSpacing { get; set; }
    public double ViewPortWidth { get; set; }
    public double ViewPortHeight { get; set; }
    public double AvgHeight { get; private set; } = -1;
    #endregion props

    public void Update(bool clear, double vp_width, double vp_height)
    {
        if (clear)
        {
            Children.Clear();
            _cachePool.Clear();
        }

        ViewPortWidth = vp_width;
        ViewPortHeight = vp_height;
        Scrolled(0, vp_width, vp_height);
    }

    public void Scrolled(double scrolledY, double vp_width, double vp_height)
    {
        double oldY = _scrollY;
        _scrollY = scrolledY;
        ViewPortWidth = vp_width;
        ViewPortHeight = vp_height;

        if (vp_height > DeviceDisplay.Current.MainDisplayInfo.Height)
            throw new InvalidOperationException("View port is very large!");

        if (vp_width > DeviceDisplay.Current.MainDisplayInfo.Width)
            throw new InvalidOperationException("View port is very large!");

        if (ItemsSource == null)
            return;

        bool isRedrawed = false;
        double oldAvg = AvgHeight;
        AvgHeight = CalcAvg();

        if (oldAvg != AvgHeight)
        {
            this.HardInvalidateMeasure();
            isRedrawed = true;
        }

        if (!isRedrawed)
        {
            //this.HardArrange(new Rect(0, 0, Width, Height));
            //this.HardInvalidateArrange(); 
            this.HardInvalidateMeasure();
        }
    }

    public Size ArrangeChildren(Rect bounds)
    {
        // resolve shift's
        double? overrideOffsetY = null;
        var shiftedEnd = new List<VirtualItem>();
        var shiftedStart = new List<VirtualItem>();
        int countRemoved = 0;

        for (int j = 0; j < _cachePool.Count; j++)
        {
            var item = _cachePool[j];
            var outRes = CalculationMethods.IsOut(item, _scrollY, ViewPortHeight);
            switch (outRes)
            {
                case OutType.Ender:
                    shiftedEnd.Add(item);
                    countRemoved++;
                    break;
                case OutType.Starter:
                    shiftedStart.Add(item);
                    countRemoved++;
                    break;
                default:
                    break;
            }
        }

        // Все элементы исчезли за пределы ViewPort'а
        if (countRemoved == _cachePool.Count && _cachePool.Count > 0)
        {
            int overrideI = CalculationMethods.CalcIndexByY(_scrollY, Height, ItemsSource.Count);
            var last = _cachePool.Last();
            last.I = overrideI;
            last.Content.BindingContext = ItemsSource[overrideI];
            last.OffsetY = _scrollY;

            shiftedStart.AddRange(shiftedEnd);
            shiftedStart.Remove(last);
            shiftedEnd.Clear();

            overrideOffsetY = _scrollY;
        }

        // Элемент исчез за пределы нижней границы ViewPort'а
        foreach (var item in shiftedEnd)
        {
            _cachePool.Remove(item);
            var second = _cachePool.First();
            int newI = second.I - 1;
            var newContext = ItemsSource[newI];
            item.I = newI;
            item.Content.BindingContext = newContext;
            item.OffsetY = second.OffsetY - item.DrawedSize.Height;
            _cachePool.Insert(0, item);
        }

        // Элемент исчез за пределы верхней границы ViewPort'а
        foreach (var item in shiftedStart)
        {
            _cachePool.Remove(item);
            var last = _cachePool.Last();
            int newI = last.I + 1;
            item.I = newI;
            item.Content.BindingContext = ItemsSource[newI];
            _cachePool.Insert(_cachePool.Count, item);
        }

        // resolve cache pool
        var template = ItemTemplate ?? _defaultItemTemplate;
        double freeViewPort = ViewPortHeight;
        double newOffsetY = overrideOffsetY ?? _cachePool.FirstOrDefault()?.OffsetY ?? 0;
        int indexPool = 0;
        while (true)
        {
            VirtualItem? cell = null;
            if (indexPool <= _cachePool.Count - 1)
            {
                cell = _cachePool[indexPool];
            }
            else if (indexPool <= ItemsSource.Count - 1)
            {
                int newI = _cachePool.Count > 0 ? _cachePool.First().I + indexPool : 0;
                var userView = template.LoadTemplate() as View;
                if (userView == null)
                    userView = new Label { Text = "INVALID_ITEM_TEMPLATE" };

                userView.BindingContext = ItemsSource[newI];
                cell = new VirtualItem(userView)
                {
                    I = newI,
                };

                _cachePool.Add(cell);
                Children.Add(cell);
            }

            if (cell is IView icell)
            {
                Size m;
                if (icell.DesiredSize.IsZero)
                    m = icell.Measure(ViewPortWidth, double.PositiveInfinity);
                else
                    m = icell.DesiredSize;

                freeViewPort -= m.Height;
                cell.OffsetY = newOffsetY;
                newOffsetY += m.Height;

                if (freeViewPort <= 0)
                {
                    break;
                }
            }
            else
            {
                break;
            }

            indexPool++;
        }

        // drawing
        var firstDraw = _cachePool.FirstOrDefault();
        if (firstDraw != null && firstDraw.I == 0)
        {
            double restartY = 0;
            foreach (var item in _cachePool)
            {
                var r = new Rect(0, restartY, bounds.Width, item.DrawedSize.Height);
                if (item is IView v)
                    v.Arrange(r);

                item.OffsetY = restartY;
                restartY += item.DrawedSize.Height;
            }
            goto end;
        }

        var lastDraw = _cachePool.LastOrDefault();
        if (lastDraw != null && lastDraw.I == ItemsSource.Count - 1)
        {
            double restartY = Height;
            for (int i = _cachePool.Count - 1; i >= 0; i--)
            {
                var item = _cachePool[i];
                double y = restartY - item.DrawedSize.Height;
                var r = new Rect(
                    x: 0,
                    y: y,
                    width: bounds.Width,
                    height: item.DrawedSize.Height);
                if (item is IView v)
                    v.Arrange(r);

                item.OffsetY = y;
                restartY -= item.DrawedSize.Height;
            }
            goto end;
        }

        double syncY = _cachePool.FirstOrDefault()?.OffsetY ?? 0;
        foreach (var item in _cachePool)
        {
            var r = new Rect(0, syncY, bounds.Width, item.DrawedSize.Height);
            if (item is IView v)
                v.Arrange(r);

            item.OffsetY = syncY;
            syncY += item.DrawedSize.Height;
        }

    end:
        if (AvgHeight <= 0)
        {
            AvgHeight = CalcAvg();
            if (this is IView selfs)
                selfs.InvalidateMeasure();
        }

        return bounds.Size;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        foreach (var item in Children)
            item.Measure(widthConstraint, heightConstraint);

        AvgHeight = CalcAvg();
        double height = Cunt * AvgHeight;
        if (height <= 0)
            height = 200;

        return new Size(widthConstraint, height);
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }

    public void Redraw(VirtualItem invoker)
    {
        if (Height <= 0)
            return;

        if (this is IView self)
        {
            self.InvalidateArrange();
        }
    }

    private double CalcAvg()
    {
        if (_cachePool.Count == 0)
            return -1;

        return _cachePool.Average(x => x.Content.Height);
    }
}
