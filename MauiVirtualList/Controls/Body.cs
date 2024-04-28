using Microsoft.Maui.Layouts;
using System.Collections;
using MauiVirtualList.Enums;
using System.Collections.ObjectModel;
using MauiVirtualList.Utils;
using System.Diagnostics;
using MauiVirtualList.Structs;

namespace MauiVirtualList.Controls;

public class Body : Layout, ILayoutManager
{
    private readonly List<VirtualItem> _cachePool = [];
    private readonly DataTemplate _defaultItemTemplate = new(() => new Label { Text = "NO_TEMPLATE" });
    private double _scrollY;
    private double _scrollY_Old;

    #region props
    // items source
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IList),
        typeof(Body),
        null,
        propertyChanged: (b, o, n) =>
        {
            if (b is Body self)
                self.Update(true, self.ViewPortWidth, self.ViewPortHeight);
        }
    );
    public IList? ItemsSource
    {
        get => GetValue(ItemsSourceProperty) as IList;
        set => SetValue(ItemsSourceProperty, value);
    }

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

    public int Cunt => ItemsSource != null ? ItemsSource.Count : 0;
    public double ItemsSpacing { get; set; }
    public double ViewPortWidth { get; set; }
    public double ViewPortHeight { get; set; }
    public double ViewPortBottomLim => _scrollY + ViewPortHeight;
    public double AvgHeight { get; private set; } = -1;
    public double EstimatedHeight => Cunt * AvgHeight;
    #endregion props

    public void Update(bool isHardUpdate, double vp_width, double vp_height)
    {
        if (isHardUpdate)
        {
            Children.Clear();
            _cachePool.Clear();

            //// init tracking
            //if (ItemsSource != null)
            //{
            //    _trackingItem = BuildCell(0);
            //    _trackingItem.HardMeasure(vp_width, double.PositiveInfinity);
            //}
        }

        ViewPortWidth = vp_width;
        ViewPortHeight = vp_height;
        Scrolled(0, vp_width, vp_height);
    }

    public void Scrolled(double scrolledY, double vp_width, double vp_height)
    {
        _scrollY_Old = _scrollY;
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

    private AnalyzeResult Analyze()
    {
        int middleOffsetStart = 0;
        int middleOffsetEnd = _cachePool.Count - 1;
        int topCacheCount = 0;
        int bottomCacheCount = 0;
        VirtualItem? middleStartItem = null;
        VirtualItem? middleEndItem = null;

        // find top cache
        for (int i = 0; i < _cachePool.Count; i++)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim);
            cell.CachedPercentVis = vis.Percent;

            if (vis.VisibleType == VisibleTypes.Starter)
            {
                middleOffsetStart++;
                topCacheCount++;
            }
            else
            {
                middleStartItem = cell;
                break;
            }
        }

        // find bottom cache
        for (int i = _cachePool.Count - 1; i >= 0; i--)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim);
            cell.CachedPercentVis = vis.Percent;

            if (vis.VisibleType == VisibleTypes.Ender)
            {
                middleOffsetEnd--;
                bottomCacheCount++;
            }
            else
            {
                middleEndItem = cell;
                break;
            }
        }

        // middle
        double freeViewPort = ViewPortHeight;
        for (int i = middleOffsetStart; i <= middleOffsetEnd; i++)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim);
            cell.CachedPercentVis = vis.Percent;

            double cellViewPortBusyHeight = cell.DrawedSize.Height * vis.Percent;
            freeViewPort -= cellViewPortBusyHeight;
        }

        return new AnalyzeResult
        {

        };
    }

    public Size ArrangeChildren(Rect bounds)
    {
        int topCacheCount = 0;
        int bottomCacheCount = 0;
        int cacheCount = 0;
        int middleOffsetStart = 0;
        int middleOffsetEnd = _cachePool.Count != 0 ? _cachePool.Count - 1 : 0;
        VirtualItem? middleStart = null;

        // find top cache
        for (int i = 0; i < _cachePool.Count; i++)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim);
            cell.CachedPercentVis = vis.Percent;

            switch (vis.VisibleType)
            {
                case VisibleTypes.Visible:
                    middleStart = cell;
                    goto endTopCache;
                case VisibleTypes.Starter:
                    middleOffsetStart++;
                    cacheCount++;
                    topCacheCount++;
                    cell.IsCacheTop = true;
                    break;
                case VisibleTypes.Ender:
                default:
                    goto endTopCache;
            }
        }
    endTopCache:

        // find bottom cache
        for (int i = _cachePool.Count - 1; i >= 0; i--)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim);
            cell.CachedPercentVis = vis.Percent;

            switch (vis.VisibleType)
            {
                case VisibleTypes.Ender:
                    middleOffsetEnd--;
                    cacheCount++;
                    bottomCacheCount++;
                    cell.IsCacheBottom = true;
                    break;
                case VisibleTypes.Visible:
                case VisibleTypes.Starter:
                default:
                    goto endBottomCache;
            }
        }
    endBottomCache:

        // resolve ViewPort cache pool (middle)
        var anchor = middleStart;
        double freeViewPort = ViewPortHeight;
        double newOffsetY = anchor?.OffsetY ?? _scrollY;
        int indexPool = middleStart != null ? _cachePool.IndexOf(middleStart) : 0;
        int indexSource = anchor?.LogicIndex ?? CalcMethods.CalcIndexByY(_scrollY, EstimatedHeight, ItemsSource.Count);
        while (true)
        {
            VirtualItem cell;
            if (indexPool <= _cachePool.Count - 1)
            {
                cell = _cachePool[indexPool];
            }
            else if (indexPool <= ItemsSource.Count - 1)
            {
                if (indexSource > ItemsSource.Count - 1)
                    break;

                if (_cachePool.Count > 30)
                    Debugger.Break();

                try
                {
                    cell = BuildCell(indexSource);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    throw;
                }
            }
            else
            {
                break;
            }

            double cellHeight;
            if (cell.DesiredSize.IsZero)
                cellHeight = cell.HardMeasure(ViewPortWidth, double.PositiveInfinity).Height;
            else
                cellHeight = cell.DesiredSize.Height;

            cell.OffsetY = newOffsetY;
            newOffsetY += cellHeight;

            var visiblePercent = CalcMethods.CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim);
            cell.CachedPercentVis = visiblePercent.Percent;

            // check cache
            if (cell.IsCache && cell.CachedPercentVis > 0)
            {
                if (cell.IsCacheTop)
                {
                    cell.IsCacheTop = false;
                    topCacheCount--;
                }
                else
                {
                    cell.IsCacheBottom = false;
                    bottomCacheCount--;
                }

                cell.Shift(indexSource, ItemsSource);

                // recalc cache
                cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                visiblePercent = CalcMethods.CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim);
                cell.CachedPercentVis = visiblePercent.Percent;

                cacheCount--;
            }

            double cellViewPortBusyHeight = cellHeight * visiblePercent.Percent;
            freeViewPort -= cellViewPortBusyHeight;

            if (freeViewPort.IsEquals(0.0, 0.001) || cellViewPortBusyHeight == 0)
            {
                Debug.WriteLine($"feeViewPort height :: {freeViewPort}");
                break;
            }

            indexPool++;
            indexSource++;
        }

        // shifle cache
        if (cacheCount >= 2)
        {
            bool topEnable = true;
            bool bottomEnable = true;

            if (cacheCount % 2 != 0)
            {
                topEnable = true;
                bottomEnable = false;
            }

            // Из начала в конец
            if (topEnable && topCacheCount > bottomCacheCount)
            {
                var pre = _cachePool.Last();
                int newIndex = pre.LogicIndex + 1;
                if (newIndex <= ItemsSource.Count - 1)
                {
                    var first = _cachePool.First();
                    _cachePool.Remove(first);
                    first.LogicIndex = newIndex;
                    first.Content.BindingContext = ItemsSource[newIndex];
                    first.OffsetY = pre.BottomLim + first.HardMeasure(ViewPortWidth, double.PositiveInfinity).Height;
                    _cachePool.Add(first);
                }
            }

            // Из конца в начало
            if (bottomEnable && bottomCacheCount > topCacheCount)
            {
                if (_cachePool.First().LogicIndex != 0)
                {
                    var last = _cachePool.Last();
                    _cachePool.Remove(last);
                    var pre = _cachePool.First();
                    last.LogicIndex = pre.LogicIndex - 1;
                    last.Content.BindingContext = ItemsSource[last.LogicIndex];
                    last.OffsetY = pre.OffsetY - last.HardMeasure(ViewPortWidth, double.PositiveInfinity).Height;
                    _cachePool.Insert(0, last);
                }
            }
        }

        // DRAW FOR FIRST
        var firstDraw = _cachePool.FirstOrDefault();
        if (firstDraw != null && firstDraw.LogicIndex == 0)
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

        // DRAW FOR LAST
        var lastDraw = _cachePool.LastOrDefault();
        if (lastDraw != null && lastDraw.LogicIndex == ItemsSource.Count - 1)
        {
            double restartY = EstimatedHeight;
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

        // DRAW FOR MIDDLE
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

    private VirtualItem BuildCell(int logicIndex)
    {
        var template = ItemTemplate ?? _defaultItemTemplate;
        var userView = template.LoadTemplate() as View;
        if (userView == null)
            userView = new Label { Text = "INVALID_ITEM_TEMPLATE" };

        userView.BindingContext = ItemsSource![logicIndex];
        var cell = new VirtualItem(userView)
        {
            LogicIndex = logicIndex,
        };
        _cachePool.Add(cell);
        Children.Add(cell);

        return cell;
    }

    private double CalcAvg()
    {
        if (_cachePool.Count == 0)
            return -1;

        return _cachePool.Average(x => x.Content.Height);
    }
}
