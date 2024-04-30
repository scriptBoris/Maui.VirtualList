using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Layouts;
using MauiVirtualList.Enums;
using MauiVirtualList.Utils;
using MauiVirtualList.Structs;

namespace MauiVirtualList.Controls;

public class Body : Layout, ILayoutManager
{
    private readonly ShifleCacheController _shifleController = new();
    private readonly List<VirtualItem> _cachePool = [];
    private readonly DataTemplate _defaultItemTemplate = new(() => new Label { Text = "NO_TEMPLATE" });
    private double _scrollY;

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
    public double AvgCellHeight { get; private set; } = -1;
    public double EstimatedHeight => Cunt * AvgCellHeight;
    #endregion props

    public void Update(bool isHardUpdate, double vp_width, double vp_height)
    {
        if (isHardUpdate)
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
        _scrollY = scrolledY;
        ViewPortWidth = vp_width;
        ViewPortHeight = vp_height;

        if (vp_height > DeviceDisplay.Current.MainDisplayInfo.Height)
            throw new InvalidOperationException("View port is very large!");

        if (vp_width > DeviceDisplay.Current.MainDisplayInfo.Width)
            throw new InvalidOperationException("View port is very large!");

        if (ItemsSource == null)
            return;

#if WINDOWS
        // У винды особое представление о рисовании элементов
        this.HardInvalidateMeasure();
#else
        // Вызываем прямую перерисовку элементов (для повышения производительности)
        ArrangeChildren(new Rect(0, 0, vp_width, Height));
#endif
    }

    public Size ArrangeChildren(Rect bounds)
    {
        if (ItemsSource == null || ViewPortWidth <= 0 || ViewPortHeight <= 0)
            return bounds.Size;

        // 1: find caches
        Caches(out VirtualItem? middleStart,
               out int middleOffsetStart, // наверное это не нужно
               out int middleOffsetEnd,   // и это
               out int cacheCount, 
               out int topCacheCount, 
               out int bottomCacheCount);

        // 2: resolve ViewPort cache pool (middle)
        // Рассчет заполненности вьюпорта элементами.
        // Данный алгоритм "смотрит вниз"
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

                // Слишком много кэша, что то пошло не так :(
                if (_cachePool.Count > 30)
                    Debugger.Break();

                if (topCacheCount > 1)
                {
                    // todo отладить, шифт без перемещения в пуле
                    cell = _cachePool.First();
                    cell.Shift(indexSource, ItemsSource);
                }
                else
                {
                    cell = BuildCell(indexSource);
                }
                anchor ??= cell;
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
            // Если во вьюпорт попали кэш элементы, то 
            // помечаем их, что они больше не КЭШе
            // TODO странный участок кода, можно укомпановать отрефакторить
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

                cacheCount--;

                cell.Shift(indexSource, ItemsSource);

                // recalc cache
                cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                visiblePercent = CalcMethods.CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim);
                cell.CachedPercentVis = visiblePercent.Percent;
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

        // 3: check "holes middle"
        // (Нужен ли данный алгоритм?)
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

            var holeSize = CalcMethods.ChekHoleTop(_scrollY, ViewPortBottomLim, anchor);
            if (holeSize > 0)
            {
                int index = anchor.LogicIndex - 1;
                int insert = _cachePool.IndexOf(anchor) - 1;
                if (insert < 0)
                    insert = 0;

                VirtualItem cell;
                if (bottomCacheCount > 1)
                {
                    cell = _cachePool.Last();
                    _cachePool.Remove(cell);
                    _cachePool.Insert(insert, cell);
                    cell.Shift(index, ItemsSource);
                }
                else
                {
                    cell = BuildCell(index, insert);
                }
                cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                cell.OffsetY = anchor.OffsetY - cell.DrawedSize.Height;
                cell.CachedPercentVis = CalcMethods.CalcVisiblePercent(cell.OffsetY, cell.BottomLim, _scrollY, ViewPortBottomLim).Percent;
                anchor = cell;
            }
            else
            {
                break;
            }
        }

        // Индекс логического элемента, который первый сверху вьюпорта
        //int middleLogicIndexStart = anchor?.LogicIndex ?? 0;

        // Индекс логического элемента, который последний снизу вьюпорта
        //int middleLogicIndexEnd = indexSource;

        // 3.1: recalc caches
        // После мидла, может оказаться так, что верхний кэш будет внизу
        // или нижний кэш будет наверху.
        // Поэтому пересчитываем КЭШи, для надежности
        CalcMethods.RecalcCache(_cachePool, _scrollY, ViewPortBottomLim,
            out int middleLogicIndexStart,
            out int middleLogicIndexEnd,
            out cacheCount,
            out topCacheCount,
            out bottomCacheCount);

        // 4: shifle cache
        // Алгоритм распределяет верхний и нижний кэш поровну
        int unsolvedCacheCount = cacheCount;
        var rule = ShifleCacheRules.Default;
        if (middleLogicIndexStart == 0)
            rule = ShifleCacheRules.NoCacheTop;
        else if (middleLogicIndexEnd == ItemsSource.Count - 1)
            rule = ShifleCacheRules.NoCacheBottom;

        _shifleController.Rule = rule;
        _shifleController.ScrollTop = _scrollY;
        _shifleController.ScrollBottom = ViewPortBottomLim;
        while (true)
        {
            bool isEnough = _shifleController.Shifle(_cachePool, ItemsSource, ViewPortWidth,
                ref unsolvedCacheCount,
                ref topCacheCount,
                ref bottomCacheCount);

            if (isEnough)
                break;
        }

        // 5: IndexLogic error correction
        // Проходимся по КЭШ элементам, если находим ошибки непоследовательности
        // ItemsSource - фиксим их
        if (_cachePool.Count > 0)
        {
            // up
            var itemCorrectionUp = _cachePool.First(x => x.CachedPercentVis > 0);
            int indexCorectionUp = itemCorrectionUp.LogicIndex;
            for (int i = _cachePool.IndexOf(itemCorrectionUp); i >= 0; i--)
            {
                var cell = _cachePool[i];
                if (cell.LogicIndex != indexCorectionUp)
                {
                    cell.Shift(indexCorectionUp, ItemsSource);
                    cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                }
                indexCorectionUp--;
            }

            // down
            var itemCorrectionDown = _cachePool.Last(x => x.CachedPercentVis > 0);
            int indexCorectionDown = itemCorrectionDown.LogicIndex;
            for (int i = _cachePool.IndexOf(itemCorrectionDown); i < _cachePool.Count; i++)
            {
                var cell = _cachePool[i];
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
                    _cachePool.Remove(cell);
                    Children.Remove(cell);
                    i--;
                }
                indexCorectionDown++;
            }
        }

        // Собственно, рисуем наш cachepool
        Draw(bounds);

        // Если размер avg изменился, то изменяем размер тела прокрутки (body)
        // (сомнительно, но-о-о окэ-э-э-й)
        if (AvgCellHeight <= 0)
        {
            AvgCellHeight = CalcAverageCellHeight();
            if (this is IView selfs)
                selfs.InvalidateMeasure();
        }

        return bounds.Size;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        foreach (var item in Children)
            item.Measure(widthConstraint, heightConstraint);

        AvgCellHeight = CalcAverageCellHeight();
        double height = Cunt * AvgCellHeight;
        if (height <= 0)
            height = 200;

        return new Size(widthConstraint, height);
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }

    private void Caches(out VirtualItem? middleStart, 
        out int middleOffsetStart, 
        out int middleOffsetEnd,
        out int cacheCount, 
        out int topCacheCount,
        out int bottomCacheCount)
    {
        // 1: find caches
        // find top cache
        middleStart = null;
        cacheCount = 0;
        middleOffsetStart = 0;
        middleOffsetEnd = _cachePool.Count != 0 ? _cachePool.Count - 1 : 0;
        topCacheCount = 0;
        bottomCacheCount = 0;

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
    endBottomCache:;
    }

    private void Draw(Rect bounds)
    {
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
            return;
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
            return;
        }

        // DRAW FOR MIDDLE
        double syncY = _cachePool.FirstOrDefault()?.OffsetY ?? 0;
        foreach (var item in _cachePool)
        {
            var r = new Rect(0, syncY, bounds.Width, item.DrawedSize.Height);
            item.HardArrange(r);
            item.OffsetY = syncY;
            syncY += item.DrawedSize.Height;
        }
    }

    private VirtualItem BuildCell(int logicIndex, int? insertIndex = null)
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

        if (insertIndex == null)
        {
            _cachePool.Add(cell);
        }
        else
        {
            int insrt = insertIndex.Value;
            if (insrt < 0)
                insrt = 0;

            _cachePool.Insert(insrt, cell);
        }

        Children.Add(cell);

        return cell;
    }

    private double CalcAverageCellHeight()
    {
        if (_cachePool.Count == 0)
            return -1;

        return _cachePool.Average(x => x.DrawedSize.Height);
    }
}
