using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Layouts;
using MauiVirtualList.Enums;
using MauiVirtualList.Utils;
using MauiVirtualList.Structs;
using System.Collections.Specialized;

namespace MauiVirtualList.Controls;

//public class Body : Layout, ILayoutManager
//{
//    private readonly ShifleCacheController _shifleController = new();
//    private readonly CacheController _cacheController = new();
//    private readonly DataTemplate _defaultItemTemplate = new(() => new Label { Text = "NO_TEMPLATE" });
//    private View? _emptyView;
//    private double _scrollY;

//    #region bindable props
//    // items source
//    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
//        nameof(ItemsSource),
//        typeof(IList),
//        typeof(Body),
//        null,
//        propertyChanged: (b, o, n) =>
//        {
//            if (b is Body self)
//            {
//                if (o is INotifyCollectionChanged old)
//                    old.CollectionChanged -= self.OnCollectionChanged;

//                if (n is INotifyCollectionChanged newest)
//                    newest.CollectionChanged += self.OnCollectionChanged;

//                self.Update(true, self.ViewPortWidth, self.ViewPortHeight);
//            }
//        }
//    );
//    public IList? ItemsSource
//    {
//        get => GetValue(ItemsSourceProperty) as IList;
//        set => SetValue(ItemsSourceProperty, value);
//    }

//    // item template
//    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
//        nameof(ItemTemplate),
//        typeof(DataTemplate),
//        typeof(Body),
//        null,
//        propertyChanged: (b, o, n) =>
//        {
//            if (b is Body self)
//                self.Update(true, self.ViewPortWidth, self.ViewPortHeight);
//        }
//    );
//    public DataTemplate? ItemTemplate
//    {
//        get => GetValue(ItemTemplateProperty) as DataTemplate;
//        set => SetValue(ItemTemplateProperty, value);
//    }

//    // group header template
//    public static readonly BindableProperty GroupHeaderTemplateProperty = BindableProperty.Create(
//        nameof(GroupHeaderTemplate),
//        typeof(DataTemplate),
//        typeof(Body),
//        null,
//        propertyChanged: (b, o, n) =>
//        {
//            if (b is Body self)
//                self.Update(true, self.ViewPortWidth, self.ViewPortHeight);
//        }
//    );
//    public DataTemplate? GroupHeaderTemplate
//    {
//        get => GetValue(GroupHeaderTemplateProperty) as DataTemplate;
//        set => SetValue(GroupHeaderTemplateProperty, value);
//    }

//    // group footer template
//    public static readonly BindableProperty GroupFooterTemplateProperty = BindableProperty.Create(
//        nameof(GroupFooterTemplate),
//        typeof(DataTemplate),
//        typeof(Body),
//        null,
//        propertyChanged: (b, o, n) =>
//        {
//            if (b is Body self)
//                self.Update(true, self.ViewPortWidth, self.ViewPortHeight);
//        }
//    );
//    public DataTemplate? GroupFooterTemplate
//    {
//        get => GetValue(GroupFooterTemplateProperty) as DataTemplate;
//        set => SetValue(GroupFooterTemplateProperty, value);
//    }

//    #endregion bindable props

//    #region props
//    public int Cunt => ItemsSource != null ? ItemsSource.Count : 0;
//    public double ItemsSpacing { get; set; }
//    public double ViewPortWidth { get; set; }
//    public double ViewPortHeight { get; set; }
//    public double ViewPortBottomLim => _scrollY + ViewPortHeight;
//    public double AvgCellHeight { get; private set; } = -1;
//    public double EstimatedHeight => Cunt * AvgCellHeight;
//    public double EstimatedHeightCache {  get; private set; } = 0;

//    /// <summary>
//    /// Прокрутил ли пользователь по конца списка или нет
//    /// </summary>
//    public bool IsScrolledToEnd
//    {
//        get
//        {
//            if (ViewPortHeight >= EstimatedHeightCache)
//                return false;

//            double perc = ViewPortHeight / EstimatedHeightCache;
//            if (perc > 0.8 && ViewPortBottomLim.IsEquals(EstimatedHeightCache, 5))
//                return true;

//            return ViewPortBottomLim.IsEquals(EstimatedHeightCache, 5);
//        }
//    }
//    #endregion props

//    public void Update(bool isHardUpdate, double vp_width, double vp_height)
//    {
//        if (isHardUpdate)
//        {
//            for (var i = Children.Count - 1; i >= 0; i--)
//            {
//                var item = Children[i];
//                if (item == _emptyView)
//                    continue;
//                Children.Remove(item);
//            }
//            _cacheController.Clear();
//            ResolveEmptyView();
//        }

//        ViewPortWidth = vp_width;
//        ViewPortHeight = vp_height;
//        Scrolled(0, vp_width, vp_height);
//    }

//    public void Scrolled(double scrolledY, double vp_width, double vp_height)
//    {
//        _scrollY = scrolledY;
//        _cacheController.UseViewFrame(ViewPortWidth, ViewPortHeight, scrolledY, scrolledY + ViewPortHeight);
//        ViewPortWidth = vp_width;
//        ViewPortHeight = vp_height;

//        if (vp_height > DeviceDisplay.Current.MainDisplayInfo.Height)
//            throw new InvalidOperationException("View port is very large!");

//        if (vp_width > DeviceDisplay.Current.MainDisplayInfo.Width)
//            throw new InvalidOperationException("View port is very large!");

//        if (ItemsSource == null)
//            return;

//#if WINDOWS
//        // У винды особое представление о рисовании элементов
//        this.HardInvalidateMeasure();
//#else
//        // Вызываем прямую перерисовку элементов (для повышения производительности)
//        ArrangeChildren(new Rect(0, 0, vp_width, Height));
//#endif
//    }

//    public double GetYItem(int logicIndex)
//    {
//        var cacheItem = _cacheController.ByIndexLogicOrDefault(logicIndex);
//        double y;
//        if (cacheItem != null)
//        {
//            y = cacheItem.OffsetY;
//        }
//        else
//        {
//            y = CalcMethods.CalcYByIndex(logicIndex, EstimatedHeight, ItemsSource.Count);
//        }

//        return y;
//    }

//    public Size ArrangeChildren(Rect bounds)
//    {
//        if (ItemsSource == null || ViewPortWidth <= 0 || ViewPortHeight <= 0)
//            return bounds.Size;

//        // 0: init view port
//        _cacheController.UseViewFrame(ViewPortWidth, ViewPortHeight, _scrollY, ViewPortBottomLim);

//        // 1: find caches
//        var middleStart = _cacheController.SearchCachesAndFirstMiddle();

//        // 2: resolve ViewPort cache pool (middle)
//        // Рассчет заполненности вьюпорта элементами.
//        // Данный алгоритм "смотрит вниз"
//        var anchor = middleStart;
//        double freeViewPort = ViewPortHeight;
//        double newOffsetY = anchor?.OffsetY ?? _scrollY;
//        int indexPool = _cacheController.IndexOf(middleStart, 0);
//        int indexSource = anchor?.LogicIndex ?? CalcMethods.CalcIndexByY(_scrollY, EstimatedHeight, ItemsSource.Count);
//        while (true)
//        {
//            VirtualItem cell;

//            if (indexPool <= _cacheController.CountIndex)
//            {
//                cell = _cacheController.ByIndexPool(indexPool);
//            }
//            else if (indexPool <= ItemsSource.Count - 1)
//            {
//                if (indexSource > ItemsSource.Count - 1)
//                    break;

//                // Слишком много кэша, что то пошло не так :(
//                if (_cacheController.Count > 30)
//                    Debugger.Break();

//                if (_cacheController.CacheCountTop > 1)
//                {
//                    cell = _cacheController.First();
//                    _cacheController.Remove(cell);
//                    _cacheController.Add(cell);
//                }
//                else
//                {
//                    cell = BuildCell(indexSource, -1);
//                }
//                anchor ??= cell;
//            }
//            else
//            {
//                break;
//            }

//            // check cache
//            // Если во вьюпорт попали кэш элементы, то 
//            // помечаем их, что они больше не в КЭШе
//            if (cell.IsCache)
//            {
//                _cacheController.NoCache(cell);
//                cell.Shift(indexSource, ItemsSource);
//            }

//            double cellHeight;
//            if (cell.DesiredSize.IsZero)
//                cellHeight = cell.HardMeasure(ViewPortWidth, double.PositiveInfinity).Height;
//            else
//                cellHeight = cell.DesiredSize.Height;

//            cell.OffsetY = newOffsetY;
//            newOffsetY += cellHeight;

//            var visiblePercent = CalcMethods.CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim);
//            cell.CachedPercentVis = visiblePercent.Percent;

//            double cellViewPortBusyHeight = cellHeight * visiblePercent.Percent;
//            freeViewPort -= cellViewPortBusyHeight;

//            if (freeViewPort.IsEquals(0.0, 0.001) || cellViewPortBusyHeight == 0)
//            {
//                Debug.WriteLine($"feeViewPort height :: {freeViewPort}");
//                break;
//            }

//            indexPool++;
//            indexSource++;
//        }

//        // 3: check "holes middle"
//        // TODO Нужен ли данный алгоритм?
//        // Иногда бывает такое, что при прокрутке наверх, может не быть
//        // кэш элементов и алгоритм выше не может сбилдить новые элементы
//        // (т.к. алгоритм "смотрит вниз")
//        // 
//        // Данный блок "смотрит вверх" и если вьюпорт сверху имеет свободное
//        // пространство, то билдит новый элемент (TODO или берет из кэша)
//        while (!freeViewPort.IsEquals(0.0, 0.001))
//        {
//            if (anchor == null || anchor.LogicIndex == 0)
//                break;

//            var holeSize = CalcMethods.ChekHoleTop(_scrollY, ViewPortBottomLim, anchor);
//            if (holeSize > 0)
//            {
//                int index = anchor.LogicIndex - 1;
//                int insert = _cacheController.IndexOf(anchor) - 1;
//                if (insert < 0)
//                    insert = 0;

//                VirtualItem cell;
//                if (_cacheController.CacheCountBottom > 1)
//                {
//                    cell = _cacheController.Last();
//                    _cacheController.Remove(cell);
//                    _cacheController.Insert(insert, cell);
//                    cell.Shift(index, ItemsSource);
//                }
//                else
//                {
//                    cell = BuildCell(index, insert);
//                }
//                cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
//                cell.OffsetY = anchor.OffsetY - cell.DrawedSize.Height;
//                cell.CachedPercentVis = CalcMethods.CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim).Percent;
//                anchor = cell;
//            }
//            else
//            {
//                break;
//            }
//        }

//        // 3.1: recalc caches
//        // После мидла, может оказаться так, что верхний кэш будет внизу
//        // или нижний кэш будет наверху.
//        // Поэтому пересчитываем КЭШи, для надежности
//        _cacheController.RecalcCache();

//        // 4: shifle cache
//        // Алгоритм распределяет верхний и нижний кэш поровну
//        int unsolvedCacheCount = _cacheController.CacheCount;
//        var rule = ShifleCacheRules.Default;
//        if (_cacheController.MiddleLogicIndexStart == 0)
//            rule = ShifleCacheRules.NoCacheTop;
//        else if (_cacheController.MiddleLogicIndexEnd == ItemsSource.Count - 1)
//            rule = ShifleCacheRules.NoCacheBottom;

//        _shifleController.Rule = rule;
//        _shifleController.ScrollTop = _scrollY;
//        _shifleController.ScrollBottom = ViewPortBottomLim;
//        while (true)
//        {
//            bool isEnough = _shifleController.Shifle2(_cacheController, ItemsSource, ref unsolvedCacheCount);

//            if (isEnough)
//                break;
//        }

//        // 5: IndexLogic error correction
//        // Проходимся по КЭШ элементам, если находим ошибки непоследовательности
//        // ItemsSource - фиксим их
//        if (_cacheController.Count > 0)
//        {
//            // up
//            var itemCorrectionUp = _cacheController.FirstVisible();
//            int indexCorectionUp = itemCorrectionUp.LogicIndex;
//            for (int i = _cacheController.IndexOf(itemCorrectionUp); i >= 0; i--)
//            {
//                var cell = _cacheController.ByIndexPool(i);
//                if (cell.LogicIndex != indexCorectionUp)
//                {
//                    cell.Shift(indexCorectionUp, ItemsSource);
//                    cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
//                }
//                indexCorectionUp--;
//            }

//            // down
//            var itemCorrectionDown = _cacheController.LastVisible();
//            int indexCorectionDown = itemCorrectionDown.LogicIndex;
//            for (int i = _cacheController.IndexOf(itemCorrectionDown); i < _cacheController.Count; i++)
//            {
//                var cell = _cacheController.ByIndexPool(i);
//                if (indexCorectionDown <= ItemsSource.Count - 1)
//                {
//                    if (cell.LogicIndex != indexCorectionDown)
//                    {
//                        cell.Shift(indexCorectionDown, ItemsSource);
//                        cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
//                    }
//                }
//                // если кэш ушел за пределы itemssource
//                else
//                {
//                    // TODO временное решение
//                    _cacheController.Remove(cell);
//                    Children.Remove(cell);
//                    i--;
//                }
//                indexCorectionDown++;
//            }
//        }

//        // Собственно, рисуем наш cachepool
//        Draw(bounds);

//        // Если размер avg изменился, то изменяем размер тела прокрутки (body)
//        // (сомнительно, но-о-о окэ-э-э-й)
//        //if (AvgCellHeight <= 0)
//        //{
//        //    AvgCellHeight = CalcAverageCellHeight();
//        //    if (this is IView selfs)
//        //        selfs.InvalidateMeasure();
//        //}

//        return bounds.Size;
//    }

//    public Size Measure(double widthConstraint, double heightConstraint)
//    {
//        var p = Parent as VirtualList;

//        _emptyView?.HardMeasure(ViewPortWidth, ViewPortHeight);

//        foreach (var item in Children)
//            item.Measure(widthConstraint, heightConstraint);

//        AvgCellHeight = CalcAverageCellHeight();
//        double height = Cunt * AvgCellHeight;
//        double mheight = p?.MeasureHeight ?? 200;
//        if (height <= 0)
//            height = mheight;

//        EstimatedHeightCache = height;
//        if (height < mheight)
//            height = mheight;

//        return new Size(widthConstraint, height);
//    }

//    protected override ILayoutManager CreateLayoutManager()
//    {
//        return this;
//    }

//    private void Draw(Rect bounds)
//    {
//        var drawItems = _cacheController.ExclusiveCachePool;

//        if (_emptyView != null)
//        {
//            var r = new Rect(0, 0, bounds.Width, bounds.Height);
//            _emptyView.HardArrange(r);
//        }

//        // DRAW FOR FIRST
//        var firstDraw = drawItems.FirstOrDefault();
//        if (firstDraw != null && firstDraw.LogicIndex == 0)
//        {
//            double restartY = 0;
//            foreach (var item in drawItems)
//            {
//                var r = new Rect(0, restartY, bounds.Width, item.DrawedSize.Height);
//                item.HardArrange(r);
//                item.OffsetY = restartY;
//                item.IsCacheTop = false;
//                item.IsCacheBottom = false;
//                restartY += item.DrawedSize.Height;
//            }
//            return;
//        }

//        // DRAW FOR LAST
//        var lastDraw = drawItems.LastOrDefault();
//        if (lastDraw != null && lastDraw.LogicIndex == ItemsSource.Count - 1)
//        {
//            double restartY = EstimatedHeight;
//            for (int i = drawItems.Count - 1; i >= 0; i--)
//            {
//                var item = drawItems[i];
//                double y = restartY - item.DrawedSize.Height;
//                var r = new Rect(
//                x: 0,
//                    y: y,
//                    width: bounds.Width,
//                    height: item.DrawedSize.Height);
//                item.HardArrange(r);
//                item.OffsetY = y;
//                item.IsCacheTop = false;
//                item.IsCacheBottom = false;
//                restartY -= item.DrawedSize.Height;
//            }
//            return;
//        }

//        // DRAW FOR MIDDLE
//        double syncY = drawItems.FirstOrDefault()?.OffsetY ?? 0;
//        foreach (var item in drawItems)
//        {
//            var r = new Rect(0, syncY, bounds.Width, item.DrawedSize.Height);
//            item.HardArrange(r);
//            item.OffsetY = syncY;
//            item.IsCacheTop = false;
//            item.IsCacheBottom = false;
//            syncY += item.DrawedSize.Height;
//        }
//    }

//    /// <summary>
//    /// </summary>
//    /// <param name="logicIndex"></param>
//    /// <param name="insertIndex">
//    /// null - интеграции в КЭШ не будет<br/>
//    /// -1 - будет добавлен в конец кэша<br/>
//    /// N - будет вставлен по индексу кэш коллекции
//    /// </param>
//    internal VirtualItem BuildCell(int logicIndex, int? insertIndex)
//    {
//        var template = ItemTemplate ?? _defaultItemTemplate;
//        var userView = template.LoadTemplate() as View;
//        if (userView == null)
//            userView = new Label { Text = "INVALID_ITEM_TEMPLATE" };

//        userView.BindingContext = ItemsSource![logicIndex];
//        var cell = new VirtualItem(userView)
//        {
//            LogicIndex = logicIndex,
//        };

//        if (insertIndex != null)
//        {
//            if (insertIndex.Value == -1)
//            {
//                _cacheController.Add(cell);
//            }
//            else
//            {
//                int insrt = insertIndex.Value;
//                if (insrt < 0)
//                    insrt = 0;

//                _cacheController.Insert(insrt, cell);
//            }
//        }

//        Children.Add(cell);

//        return cell;
//    }

//    internal void ResolveEmptyView()
//    {
//        bool requestFrag = (ItemsSource == null || ItemsSource.Count == 0);
//        bool currentFlag = _emptyView != null && _emptyView.IsVisible;

//        var parent = (VirtualList)Parent;
//        if (parent.EmptyViewTemplate == null)
//            requestFrag = false;
        
//        if (requestFrag == currentFlag)
//            return;

//        if (requestFrag)
//        {
//            _emptyView = parent.EmptyViewTemplate!.CreateContent() as View;
//            Children.Add(_emptyView);
//        }
//        else
//        {
//            Children.Remove(_emptyView);
//            _emptyView = null;
//        }
//    }

//    //internal void OffsetScrollAsRat(double scrollYAddValue)
//    //{
//    //    _scrollY += scrollYAddValue;
//    //    _cacheController.UseViewFrame(ViewPortWidth, ViewPortHeight, _scrollY, ViewPortBottomLim);
//    //    Frame = new Rect(0, 0, ViewPortWidth, ViewPortBottomLim);
//    //    _scrollView.ScrollToAsync(0, _scrollY, false);
//    //}

//    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
//    {
//        var collection = (IList)sender!;
//        double changeHeight = 0;
//        bool countChanged = true;
//        switch (e.Action)
//        {
//            case NotifyCollectionChangedAction.Add:
//                int newItemIndex = e.NewStartingIndex;
//                changeHeight = _cacheController.InsertCell(newItemIndex, collection, this);
//                break;
//            case NotifyCollectionChangedAction.Remove:
//                int rmItemIndex = e.OldStartingIndex;
//                var del = _cacheController.RemoveCell(rmItemIndex, collection, this);
//                if (del != null)
//                    Children.Remove(del);
//                break;
//            case NotifyCollectionChangedAction.Replace:
//                countChanged = false;
//                break;
//            case NotifyCollectionChangedAction.Move:
//                countChanged = false;
//                break;
//            case NotifyCollectionChangedAction.Reset:
//                _cacheController.Clear();
//                Clear();
//                break;
//            default:
//                break;
//        }

//        if (changeHeight != 0)
//            this.HardInvalidateMeasure();

//        if (countChanged)
//            this.ResolveEmptyView();
//    }

//    private double CalcAverageCellHeight()
//    {
//        return _cacheController.CalcAverageCellHeight();
//    }
//}
