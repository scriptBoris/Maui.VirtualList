using Microsoft.Maui.Layouts;
using System.Collections;
using MauiVirtualList.Enums;
using System.Collections.ObjectModel;
using MauiVirtualList.Utils;

namespace MauiVirtualList.Controls;

public class Body2 : Layout, ILayoutManager
{
    private readonly List<VirtualItem> _cachePool = [];
    private readonly DataTemplate _defaultItemTemplate = new(() => new Label { Text = "NO_TEMPLATE" });
    private double _scrollY;

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

    public void Update(bool clear, double vp_width, double vp_height)
    {
        if (clear)
        {
            Children.Clear();
            _cachePool.Clear();
        }

        ViewPortWidth = vp_width;
        ViewPortHeight = vp_height;
        Scrolled(0, ViewPortHeight);
    }

    public void Scrolled(double scrolledY, double viewPort)
    {
        double oldY = _scrollY;
        _scrollY = scrolledY;

        if (viewPort > DeviceDisplay.Current.MainDisplayInfo.Height)
            throw new InvalidOperationException("View port is very large!");

        if (ItemsSource == null)
            return;

        // resolve shift's
        double? overrideOffsetY = null;
        if (oldY != scrolledY)
        {
            var shiftedEnd = new List<VirtualItem>();
            var shiftedStart = new List<VirtualItem>();
            int countRemoved = 0;

            for (int j = 0; j < _cachePool.Count; j++)
            {
                var item = _cachePool[j];
                var outRes = CalculationMethods.IsOut(item, scrolledY, viewPort);
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

            int overrideI = -1;
            if (countRemoved == _cachePool.Count)
            {
                overrideI = CalculationMethods.CalcIndexByY(scrolledY, Height, ItemsSource.Count);
                var last = _cachePool.Last();
                last.I = overrideI;
                last.Content.BindingContext = ItemsSource[overrideI];
                last.OffsetY = scrolledY;

                shiftedStart.AddRange(shiftedEnd);
                shiftedStart.Remove(last);
                shiftedEnd.Clear();

                overrideOffsetY = scrolledY;
            }

            foreach (var item in shiftedEnd)
            {
                _cachePool.Remove(item);
                var second = _cachePool.First();
                int newI = second.I - 1;
                item.I = newI;
                item.Content.BindingContext = ItemsSource[newI];
                item.OffsetY = second.OffsetY - item.DrawedSize.Height;
                _cachePool.Insert(0, item);
            }

            foreach (var item in shiftedStart)
            {
                _cachePool.Remove(item);
                var last = _cachePool.Last();
                int newI = last.I + 1;
                item.I = newI;
                item.Content.BindingContext = ItemsSource[newI];
                _cachePool.Insert(_cachePool.Count, item);
            }
        }

        // resolve cache pool
        var template = ItemTemplate ?? _defaultItemTemplate;
        double freeViewPort = viewPort;
        double newOffsetY = overrideOffsetY ?? _cachePool.FirstOrDefault()?.OffsetY ?? 0;
        int i = 0;
        while (true)
        {
            VirtualItem? cell = null;
            if (i <= _cachePool.Count - 1)
            {
                cell = _cachePool[i];
            }
            else if (i <= ItemsSource.Count - 1)
            {
                int newI = _cachePool.Count > 0 ? _cachePool.First().I + i : 0;
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
                var m = icell.Measure(ViewPortWidth, double.PositiveInfinity);
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

            i++;
        }

        double oldAvg = AvgHeight;
        AvgHeight = CalcAvg();

        if (oldAvg != AvgHeight && this is IView selfs)
            selfs.InvalidateMeasure();

        if (this is IView self)
            self.InvalidateArrange();
    }

    public Size ArrangeChildren(Rect bounds)
    {
        var first = _cachePool.FirstOrDefault();
        if (first != null && first.I == 0)
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

        var last = _cachePool.LastOrDefault();
        if (last != null && last.I == ItemsSource.Count - 1)
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
        double height = Cunt * AvgHeight;
        if (height <= 0)
            height = 200;

        foreach (var item in Children)
            item.Measure(widthConstraint, heightConstraint);

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