using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui.Layouts;
using MauiVirtualList.Enums;
using MauiVirtualList.Utils;
using MauiVirtualList.Structs;

namespace MauiVirtualList.Controls;

public class Body : Layout, ILayoutManager
{
    private readonly ShifleCacheController _shifleController = new();
    private readonly CacheController _cacheController = new();
    private readonly DataTemplate _defaultItemTemplate = new(() => new Label { Text = "NO_TEMPLATE" });
    private readonly IScroller _scroller;
    private Size _redrawCache;
    private View? _emptyView;
    private SourceProvider? ItemsSource;

    internal Body(IScroller scroller)
    {
        _scroller = scroller;
    }

    #region bindable props
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

    // group header template
    public static readonly BindableProperty GroupHeaderTemplateProperty = BindableProperty.Create(
        nameof(GroupHeaderTemplate),
        typeof(DataTemplate),
        typeof(Body),
        null,
        propertyChanged: (b, o, n) =>
        {
            if (b is Body self)
                self.Update(true, self.ViewPortWidth, self.ViewPortHeight);
        }
    );
    public DataTemplate? GroupHeaderTemplate
    {
        get => GetValue(GroupHeaderTemplateProperty) as DataTemplate;
        set => SetValue(GroupHeaderTemplateProperty, value);
    }

    // group footer template
    public static readonly BindableProperty GroupFooterTemplateProperty = BindableProperty.Create(
        nameof(GroupFooterTemplate),
        typeof(DataTemplate),
        typeof(Body),
        null,
        propertyChanged: (b, o, n) =>
        {
            if (b is Body self)
            {
                self.Update(true, self.ViewPortWidth, self.ViewPortHeight);
            }
        }
    );
    public DataTemplate? GroupFooterTemplate
    {
        get => GetValue(GroupFooterTemplateProperty) as DataTemplate;
        set => SetValue(GroupFooterTemplateProperty, value);
    }

    #endregion bindable props

    #region props
    public double ScrollY => _scroller.ScrollY;
    public int Cunt => ItemsSource != null ? ItemsSource.Count : 0;
    public double ItemsSpacing { get; set; }
    public double ViewPortWidth => _scroller.ViewPortWidth;
    public double ViewPortHeight => _scroller.ViewPortHeight;
    public double ViewPortBottomLim => ScrollY + ViewPortHeight;

    public bool RequestResize { get; private set; } = true;
    public bool RequestRedraw { get; private set; } = true;
    public double AvgCellHeight { get; private set; } = -1;
    public double EstimatedHeight { get; private set; } = -1;

    // TODO Убрать, вставка идет норм без этого
    [Obsolete("Убрать")]
    /// <summary>
    /// Прокрутил ли пользователь по конца списка или нет
    /// </summary>
    public bool IsScrolledToEnd
    {
        get
        {
            if (ViewPortHeight >= EstimatedHeight)
                return false;

            double perc = ViewPortHeight / EstimatedHeight;
            if (perc > 0.8 && ViewPortBottomLim.IsEquals(EstimatedHeight, 5))
                return true;

            return ViewPortBottomLim.IsEquals(EstimatedHeight, 5);
        }
    }

    /// <summary>
    /// 0.0 - top<br/>
    /// 1.0 - bottom
    /// </summary>
    public double PercentOfScrolled
    {
        get
        {
            if (ViewPortHeight >= EstimatedHeight)
                return 0;

            double perc = _scroller.ScrollY / EstimatedHeight - ViewPortHeight;
            
            if (perc < 0)
                perc = 0;
            else if (perc > 1)
                perc = 1;

            return perc;
        }
    }
    #endregion props

    internal void SetupItemsSource(IList? oldSource, IList? newSource)
    {
        if (ItemsSource != null)
        {
            ItemsSource.ItemsAdded -= ItemsSource_ItemsAdded;
            ItemsSource.ItemsRemoved -= ItemsSource_ItemsRemoved;
            ItemsSource.ItemsCleared -= ItemsSource_ItemsCleared;
            ItemsSource.Dispose();
        }

        if (newSource != null)
        {
            ItemsSource = new(newSource, GroupHeaderTemplate != null, GroupFooterTemplate != null);
            ItemsSource.ItemsAdded += ItemsSource_ItemsAdded;
            ItemsSource.ItemsRemoved += ItemsSource_ItemsRemoved;
            ItemsSource.ItemsCleared += ItemsSource_ItemsCleared;
        }

        Update(true, ViewPortWidth, ViewPortHeight);
    }

    public void Update(bool isHardUpdate, double vp_width, double vp_height)
    {
        if (isHardUpdate)
        {
            for (var i = Children.Count - 1; i >= 0; i--)
            {
                var item = Children[i];
                if (item == _emptyView)
                    continue;
                Children.Remove(item);
            }

            _cacheController.Clear();
            ItemsSource?.Recalc(GroupHeaderTemplate != null, GroupFooterTemplate != null);
            ResolveEmptyView();

            RequestRedraw = true;
            RequestResize = true;
            this.HardInvalidateMeasure();
        }
        else
        {
            Redraw();
        }
    }

    public void Scrolled(double scrolledY, double vp_width, double vp_height)
    {
        _cacheController.UseViewFrame(ViewPortWidth, ViewPortHeight, scrolledY, ViewPortBottomLim);

        if (vp_height > DeviceDisplay.Current.MainDisplayInfo.Height)
            throw new InvalidOperationException("View port is very large!");

        if (vp_width > DeviceDisplay.Current.MainDisplayInfo.Width)
            throw new InvalidOperationException("View port is very large!");

        if (ItemsSource == null)
            return;

        // todo сделать для винды отрисовку напрямую, но перед этим измерять размеры
#if WINDOWS
        // У винды особое представление о рисовании элементов
        this.HardInvalidateMeasure();
#else
        // если есть элементы за offsetY < 0
        double outTopOffsetY = _cacheController.OutTopOffsetY();
        if (outTopOffsetY > 0)
        {
            _cacheController.FixOffsetY(outTopOffsetY);
            
            EstimatedHeight += outTopOffsetY;
            this.HardInvalidateMeasure();

            _scroller.SetScrollY(scrolledY + outTopOffsetY);
            return;
        }

        // если есть элементы которые вышли за пределы scrollView
        double outbottomOffsetY = _cacheController.OutBottomOffsetY(EstimatedHeight);
        if (outbottomOffsetY > 0)
        {
            EstimatedHeight += outbottomOffsetY;
            this.HardInvalidateMeasure();
            return;
        }

        // Вызываем прямую перерисовку элементов (для повышения производительности)
        Redraw();
#endif
    }

    public double GetYItem(int logicIndex)
    {
        var cacheItem = _cacheController.ByIndexLogicOrDefault(logicIndex);
        double y;
        if (cacheItem != null)
        {
            y = cacheItem.OffsetY;
        }
        else
        {
            y = CalcMethods.CalcYByIndex(logicIndex, EstimatedHeight, ItemsSource.Count);
        }

        return y;
    }

    public Size ArrangeChildren(Rect bounds)
    {
        double cacheW = _redrawCache.Width;
        double cacheH = _redrawCache.Height;
        double reqW = bounds.Size.Width;
        double reqH = bounds.Size.Height;
        bool useCache = cacheW.IsEquals(reqW) && cacheH.IsEquals(reqH);

        if (RequestRedraw)
            useCache = false;

        if (useCache)
            return _redrawCache;

        var drawSize = Redraw();
        RequestRedraw = false;
        return drawSize;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        double height = EstimatedHeight;
        double measureViewPortWidth = _scroller.MeasureViewPortWidth;
        double measureViewPortHeight = _scroller.MeasureViewPortHeight;

        if (RequestResize)
        {
            _emptyView?.HardMeasure(widthConstraint, heightConstraint);

            var items = _cacheController.ExclusiveCachePool;
            foreach (var item in items)
                item.HardMeasure(widthConstraint, heightConstraint);

            if (AvgCellHeight < 0 && Cunt > 0)
            {
                InitStartedCells(measureViewPortWidth, measureViewPortHeight);
                height = RecalcEstimateHeight(measureViewPortWidth, measureViewPortHeight);
            }

            RequestResize = false;
        }

        return new Size(widthConstraint, height);
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }

    private Size Redraw()
    {
        if (ItemsSource == null || ViewPortWidth <= 0 || ViewPortHeight <= 0 || EstimatedHeight <= 0)
        {
            var rect = new Rect(0, 0, ViewPortWidth, ViewPortHeight);
            Draw(rect);
            return rect.Size;
        }

        // 0: init view port
        _cacheController.UseViewFrame(ViewPortWidth, ViewPortHeight, ScrollY, ViewPortBottomLim);

        // 1: find caches
        var middleStart = _cacheController.SearchCachesAndFirstMiddle();

        // 2: resolve ViewPort cache pool (middle)
        // Рассчет заполненности вьюпорта элементами.
        // Данный алгоритм "смотрит вниз"
        var anchor = middleStart;
        double freeViewPort = ViewPortHeight;
        double newOffsetY = anchor?.OffsetY ?? ScrollY;
        int indexPool = _cacheController.IndexOf(middleStart, 0);
        int indexSource = anchor?.LogicIndex ?? CalcMethods.CalcIndexByY(ScrollY, EstimatedHeight, ItemsSource.Count);

        while (true)
        {
            VirtualItem cell;

            if (indexPool <= _cacheController.CountIndex)
            {
                cell = _cacheController.ByIndexPool(indexPool);
            }
            else if (indexPool <= ItemsSource.Count - 1)
            {
                if (indexSource > ItemsSource.Count - 1)
                    break;

                // Слишком много кэша, что то пошло не так :(
                if (_cacheController.Count > 30)
                    Debugger.Break();

                if (_cacheController.CacheCountTop > 1)
                {
                    cell = _cacheController.First();
                    _cacheController.Remove(cell);
                    _cacheController.Add(cell);
                }
                else
                {
                    cell = BuildCell(indexSource, -1);
                }
                anchor ??= cell;
            }
            else
            {
                break;
            }

            // check cache
            // Если во вьюпорт попали кэш элементы, то 
            // помечаем их, что они больше не в КЭШе
            if (cell.IsCache)
            {
                _cacheController.NoCache(cell);

                if (indexSource >= ItemsSource.Count)
                    Debugger.Break();

                cell.Shift(indexSource, ItemsSource);
            }

            double cellHeight;
            if (cell.AwaitRecalcMeasure)
                cellHeight = cell.HardMeasure(ViewPortWidth, double.PositiveInfinity).Height;
            else
                cellHeight = cell.DrawedSize.Height;

            cell.OffsetY = newOffsetY;
            newOffsetY += cellHeight;

            var visiblePercent = CalcMethods.CalcVisiblePercent(cell.OffsetY, cell.BottomLim, ScrollY, ViewPortBottomLim);
            cell.CachedPercentVis = visiblePercent.Percent;

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

        // 3: check "holes middle"
        // TODO Нужен ли данный алгоритм?
        // Иногда бывает такое, что при прокрутке наверх, может не быть
        // кэш элементов и алгоритм выше не может сбилдить новые элементы
        // (т.к. алгоритм "смотрит вниз")
        // 
        // Данный блок "смотрит вверх" и если вьюпорт сверху имеет свободное
        // пространство, то билдит новый элемент (TODO или берет из кэша)
        while (!freeViewPort.IsEquals(0.0, 0.001))
        {
            if (anchor == null || anchor.LogicIndex == 0)
                break;

            var holeSize = CalcMethods.ChekHoleTop(ScrollY, ViewPortBottomLim, anchor);
            if (holeSize > 0)
            {
                int index = anchor.LogicIndex - 1;
                int insert = _cacheController.IndexOf(anchor) - 1;
                if (insert < 0)
                    insert = 0;

                VirtualItem cell;
                if (_cacheController.CacheCountBottom > 1)
                {
                    cell = _cacheController.Last();
                    _cacheController.Remove(cell);
                    _cacheController.Insert(insert, cell);
                    cell.Shift(index, ItemsSource);
                }
                else
                {
                    cell = BuildCell(index, insert);
                }

                cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                cell.OffsetY = anchor.OffsetY - cell.DrawedSize.Height;
                cell.CachedPercentVis = CalcMethods.CalcVisiblePercent(cell.OffsetY, cell.BottomLim, ScrollY, ViewPortBottomLim).Percent;
                anchor = cell;
            }
            else
            {
                break;
            }
        }

        // 3.1: recalc caches
        // После мидла, может оказаться так, что верхний кэш будет внизу
        // или нижний кэш будет наверху.
        // Поэтому пересчитываем КЭШи, для надежности
        _cacheController.RecalcCache();

        // 4: shifle cache
        // Алгоритм распределяет верхний и нижний кэш поровну
        int unsolvedCacheCount = _cacheController.CacheCount;
        var rule = ShifleCacheRules.Default;
        if (_cacheController.MiddleLogicIndexStart == 0)
            rule = ShifleCacheRules.NoCacheTop;
        else if (_cacheController.MiddleLogicIndexEnd == ItemsSource.Count - 1)
            rule = ShifleCacheRules.NoCacheBottom;

        _shifleController.Rule = rule;
        _shifleController.ScrollTop = ScrollY;
        _shifleController.ScrollBottom = ViewPortBottomLim;
        while (true)
        {
            bool isEnough = _shifleController.Shifle2(_cacheController, ItemsSource, ref unsolvedCacheCount);

            if (isEnough)
                break;
        }

        // 5: IndexLogic error correction
        // Проходимся по КЭШ элементам, если находим ошибки непоследовательности
        // ItemsSource - фиксим их
        if (_cacheController.Count > 0)
        {
            // up
            var itemCorrectionUp = _cacheController.FirstVisible();
            int indexCorectionUp = itemCorrectionUp.LogicIndex;
            for (int i = _cacheController.IndexOf(itemCorrectionUp); i >= 0; i--)
            {
                var cell = _cacheController.ByIndexPool(i);
                if (cell.LogicIndex != indexCorectionUp)
                {
                    cell.Shift(indexCorectionUp, ItemsSource);
                    cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                }
                indexCorectionUp--;
            }

            // down
            var itemCorrectionDown = _cacheController.LastVisible();
            int indexCorectionDown = itemCorrectionDown.LogicIndex;
            for (int i = _cacheController.IndexOf(itemCorrectionDown); i < _cacheController.Count; i++)
            {
                var cell = _cacheController.ByIndexPool(i);
                if (indexCorectionDown <= ItemsSource.Count - 1)
                {
                    if (cell.LogicIndex != indexCorectionDown)
                    {
                        cell.Shift(indexCorectionDown, ItemsSource);
                        cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                    }
                }
                // если кэш ушел за пределы itemssource
                else
                {
                    // TODO временное решение
                    _cacheController.Remove(cell);
                    Children.Remove(cell);
                    i--;
                }
                indexCorectionDown++;
            }
        }

        // Собственно, рисуем наш cachepool
        Draw(new Rect(0,0, ViewPortWidth, ViewPortHeight));

        //// TODO приудмать с этим что-то
        //bool isNoEnd = false;
        //var lst = _cacheController.LastOrDefault();
        //if (lst != null)
        //{
        //    double lstDelta = EstimatedHeight - lst.BottomLim;
        //    isNoEnd = lstDelta < -50 && lst.LogicIndex != ItemsSource.Count - 1;
        //}

        //if (isNoEnd)
        //{
        //    //int dif = ItemsSource.Count - lst.LogicIndex;
        //    EstimatedHeight = EstimatedHeight + 20;
        //    Debug.WriteLine($"OVERRIDED EstimatedHeight + 20!");
        //}

        _redrawCache = new Size(ViewPortWidth, ViewPortHeight);
        return _redrawCache;
    }

    private void Draw(Rect bounds)
    {
        if (_emptyView != null)
        {
            var r = new Rect(0, 0, bounds.Width, bounds.Height);
            _emptyView.HardArrange(r);
        }

        var drawItems = _cacheController.ExclusiveCachePool;

        // DRAW FOR FIRST
        var firstDraw = drawItems.FirstOrDefault();
        if (firstDraw != null && firstDraw.LogicIndex == 0)
        {
            double restartY = 0;
            foreach (var item in drawItems)
            {
                var r = new Rect(0, restartY, bounds.Width, item.DrawedSize.Height);
                item.HardArrange(r);
                item.OffsetY = restartY;
                item.IsCacheTop = false;
                item.IsCacheBottom = false;
                restartY += item.DrawedSize.Height;
            }
            return;
        }

        // DRAW FOR LAST
        var lastDraw = drawItems.LastOrDefault();
        if (lastDraw != null && lastDraw.LogicIndex == ItemsSource.Count - 1)
        {
            double restartY = EstimatedHeight;
            for (int i = drawItems.Count - 1; i >= 0; i--)
            {
                var item = drawItems[i];
                double y = restartY - item.DrawedSize.Height;
                var r = new Rect(
                x: 0,
                    y: y,
                    width: bounds.Width,
                    height: item.DrawedSize.Height);
                item.HardArrange(r);
                item.OffsetY = y;
                item.IsCacheTop = false;
                item.IsCacheBottom = false;
                restartY -= item.DrawedSize.Height;
            }
            return;
        }

        // DRAW FOR MIDDLE
        double syncY = drawItems.FirstOrDefault()?.OffsetY ?? 0;
        foreach (var item in drawItems)
        {
            var r = new Rect(0, syncY, bounds.Width, item.DrawedSize.Height);
            item.HardArrange(r);
            item.OffsetY = syncY;
            item.IsCacheTop = false;
            item.IsCacheBottom = false;
            syncY += item.DrawedSize.Height;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="logicIndex"></param>
    /// <param name="insertIndex">
    /// null - интеграции в КЭШ не будет<br/>
    /// -1 - будет добавлен в конец кэша<br/>
    /// N - будет вставлен по индексу кэш коллекции
    /// </param>
    internal VirtualItem BuildCell(int logicIndex, int? insertIndex)
    {
        var context = ItemsSource![logicIndex];
        var templateType = ItemsSource.GetTypeItem(logicIndex);
        bool isRequired;
        DataTemplate? template;

        switch (templateType)
        {
            case DoubleTypes.Header:
                template = GroupHeaderTemplate;
                isRequired = false;
                break;
            case DoubleTypes.Item:
                template = ItemTemplate ?? _defaultItemTemplate;
                isRequired = true;
                break;
            case DoubleTypes.Footer:
                template = GroupFooterTemplate;
                isRequired = false;
                break;
            default:
                throw new InvalidOperationException();
        }

        var userView = template?.LoadTemplate() as View;
        if (userView == null)
        {
            if (isRequired)
                userView = new Label { Text = "INVALID_ITEM_TEMPLATE" };
            else
                throw new InvalidOperationException();
        }

        userView.BindingContext = context;
        var cell = new VirtualItem(userView, templateType)
        {
            LogicIndex = logicIndex,
        };

        if (insertIndex != null)
        {
            if (insertIndex.Value == -1)
            {
                _cacheController.Add(cell);
            }
            else
            {
                int insrt = insertIndex.Value;
                if (insrt < 0)
                    insrt = 0;

                _cacheController.Insert(insrt, cell);
            }
        }

        Children.Add(cell);

        return cell;
    }

    internal void ResolveEmptyView()
    {
        bool requestFrag = (ItemsSource == null || ItemsSource.Count == 0);
        bool currentFlag = _emptyView != null && _emptyView.IsVisible;

        var parent = (VirtualList)Parent;
        if (parent.EmptyViewTemplate == null)
            requestFrag = false;
        
        if (requestFrag == currentFlag)
            return;

        if (requestFrag)
        {
            _emptyView = parent.EmptyViewTemplate!.CreateContent() as View;
            Children.Add(_emptyView);
        }
        else
        {
            Children.Remove(_emptyView);
            _emptyView = null;
        }
    }

    private void ItemsSource_ItemsAdded(int wideindexStart, object[] items)
    {
        var collection = ItemsSource!;
        _cacheController.InsertCells(wideindexStart, items, collection, (id) => BuildCell(id, null),
            out double rmHeight,
            out double changedScrollY);

        this.ResolveEmptyView();

        AvgCellHeight = CalcAverageCellHeight();
        EstimatedHeight += rmHeight;

        // todo сделать отрисовку правильной
        RequestResize = true;
        this.HardInvalidateMeasure();

        Redraw();
    }

    private void ItemsSource_ItemsRemoved(int wideindexStart, int[] deletedIndexes)
    {
        var collection = ItemsSource!;
        var result = _cacheController.RemoveCells(
            wideindexStart, 
            deletedIndexes, 
            collection,
            EstimatedHeight,
            ScrollY
        );

        foreach (var item in result.DeleteItems)
            Children.Remove(item);

        bool mustRedraw = result.MustBeRedraw;
        bool wasRedraw = false;

        if (result.DeleteItems.Length > 0)
            ResolveEmptyView();

        if (result.NewBodyHeight != null)
        {
            AvgCellHeight = CalcAverageCellHeight();
            EstimatedHeight = result.NewBodyHeight.Value;
            RequestResize = true;
            this.HardInvalidateMeasure();
        }

        if (result.NewScrollY != null)
        {
            RequestRedraw = true;
            RequestResize = true;
            _scroller.SetScrollY(result.NewScrollY.Value);
            wasRedraw = true;
        }

        if (mustRedraw && !wasRedraw)
            Redraw();
    }

    private void ItemsSource_ItemsCleared()
    {
        foreach (var item in _cacheController.ExclusiveCachePool)
            Children.Remove(item);

        _cacheController.Clear();

        this.ResolveEmptyView();

        RequestRedraw = true;
        RequestResize = true;

        _scroller.SetScrollY(0);
        this.HardInvalidateMeasure();
        Redraw();
    }

    public double RecalcEstimateHeight(double vpWidth, double vpHeight)
    {
        AvgCellHeight = CalcAverageCellHeight();
        double result = Cunt * AvgCellHeight;
        if (result <= 0)
            result = vpHeight;

        EstimatedHeight = result;
        if (result < vpHeight)
            result = vpHeight;

        return result;
    }

    private double CalcAverageCellHeight()
    {
        double res = _cacheController.CalcAverageCellHeight(ItemsSource);
        Debug.WriteLine($"AVG height: {res}");
        return res;
    }

    private void InitStartedCells(double vpwidth, double vpheight)
    {
        int index = 0;
        double newOffsetY = 0;
        double freeViewPort = vpheight;
        double viewPortBottomLim = ScrollY + vpheight;
        int maxIndex = ItemsSource!.Count - 1;

        while (true)
        {
            if (index > maxIndex)
                break;

            if (freeViewPort <= 0) 
                break;

            var cell = BuildCell(index, -1);
            cell.HardMeasure(vpwidth, double.PositiveInfinity);
            cell.OffsetY = newOffsetY;
            cell.CachedPercentVis = CalcMethods.CalcVisiblePercent(cell.OffsetY, cell.BottomLim, ScrollY, viewPortBottomLim).Percent;
            
            newOffsetY += cell.DrawedSize.Height;
            freeViewPort -= cell.DrawedSize.Height;
            index++;
        }
    }
}