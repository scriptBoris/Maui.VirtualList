using System.Collections;
using System.Diagnostics;
using MauiVirtualList.Controls;
using MauiVirtualList.Enums;
using MauiVirtualList.Structs;

namespace MauiVirtualList.Utils;

internal class CacheController
{
    private readonly List<VirtualItem> _cachePool = [];
    private readonly Random _random = new();

    public IReadOnlyList<VirtualItem> ExclusiveCachePool => _cachePool;
    public double ViewPortWidth { get; private set; }
    public double ViewPortHeight { get; private set; }
    public double ViewPortFillPercent { get; private set; }
    public double ScrollTop { get; private set; }
    public double ScrollBottom { get; private set; }
    public double AvgCellHeight { get; private set; } = -1;

    public int CacheCountTop { get; private set; }
    public int CacheCountBottom { get; private set; }
    public int Count => _cachePool.Count;

    public VirtualItem? Middle { get; private set; }
    public int MiddleLogicIndexStart { get; private set; }
    public int MiddleLogicIndexEnd { get; private set; }


    /// <summary>
    /// _cachePool.Count - 1
    /// </summary>
    public int CountIndex => _cachePool.Count - 1;
    public int CacheCount => CacheCountTop + CacheCountBottom;

    public int IndexOf(VirtualItem item)
    {
        return _cachePool.IndexOf(item);
    }

    public int IndexOf(VirtualItem? item, int defaultIndex = -1)
    {
        if (item == null)
            return defaultIndex;

        int res = _cachePool.IndexOf(item);
        if (res == -1 && defaultIndex != -1)
            return defaultIndex;
        return res;
    }

    public VirtualItem ByIndexPool(int indexPool)
    {
        return _cachePool[indexPool];
    }

    public VirtualItem ByIndexLogic(int indexLogic)
    {
        return _cachePool.First(x => x.LogicIndex == indexLogic);
    }

    public VirtualItem? ByIndexLogicOrDefault(int indexLogic)
    {
        return _cachePool.FirstOrDefault(x => x.LogicIndex == indexLogic);
    }

    public VirtualItem First()
    {
        return _cachePool.First();
    }

    public VirtualItem FirstVisible()
    {
        return _cachePool.First(x => x.CachedPercentVis > 0);
    }

    public VirtualItem Last()
    {
        return _cachePool.Last();
    }

    public VirtualItem? LastOrDefault()
    {
        return _cachePool.LastOrDefault();
    }

    public VirtualItem LastVisible()
    {
        return _cachePool.Last(x => x.CachedPercentVis > 0);
    }

    public void Remove(VirtualItem item)
    {
        _cachePool.Remove(item);
    }

    public void Add(VirtualItem item)
    {
        _cachePool.Add(item);
    }

    public void Insert(int index, VirtualItem item)
    {
        _cachePool.Insert(index, item);
    }

    public void NoCache(VirtualItem item)
    {
        if (item.IsCacheTop)
        {
            item.IsCacheTop = false;
            CacheCountTop--;
        }
        else
        {
            item.IsCacheBottom = false;
            CacheCountBottom--;
        }
    }

    public void UseViewFrame(double viewportWidth, double viewportHeight, double scrollTop, double scrollBottom)
    {
        ViewPortWidth = viewportWidth;
        ViewPortHeight = viewportHeight;
        ScrollTop = scrollTop;
        ScrollBottom = scrollBottom;
    }

    public VirtualItem? SearchCachesAndFirstMiddle()
    {
        CacheCountTop = 0;
        CacheCountBottom = 0;
        Middle = null;
        MiddleLogicIndexStart = 0;
        MiddleLogicIndexEnd = 0;

        for (int i = 0; i < _cachePool.Count; i++)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, ScrollTop, ScrollBottom);
            cell.CachedPercentVis = vis.Percent;

            switch (vis.VisibleType)
            {
                case VisibleTypes.Visible:
                    Middle = cell;
                    goto endTopCache;
                case VisibleTypes.Starter:
                    CacheCountTop++;
                    cell.IsCacheTop = true;
                    break;
                case VisibleTypes.Ender:
                default:
                    goto endTopCache;
            }
        }
    endTopCache: { }

        // find bottom cache
        for (int i = _cachePool.Count - 1; i >= 0; i--)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, ScrollTop, ScrollBottom);
            cell.CachedPercentVis = vis.Percent;

            switch (vis.VisibleType)
            {
                case VisibleTypes.Ender:
                    CacheCountBottom++;
                    cell.IsCacheBottom = true;
                    break;
                case VisibleTypes.Visible:
                case VisibleTypes.Starter:
                default:
                    goto endBottomCache;
            }
        }
    endBottomCache: { }

        return Middle;
    }

    internal void RecalcCache()
    {
        CacheCountTop = 0;
        CacheCountBottom = 0;
        Middle = null;
        MiddleLogicIndexStart = -1;
        MiddleLogicIndexEnd = -1;

        double visibleFillHeight = 0;

        for (int i = 0; i < _cachePool.Count; i++)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, ScrollTop, ScrollBottom);
            cell.CachedPercentVis = vis.Percent;

            switch (vis.VisibleType)
            {
                case VisibleTypes.Visible:
                    if (MiddleLogicIndexStart == -1)
                        MiddleLogicIndexStart = cell.LogicIndex;
                    else if (MiddleLogicIndexEnd == -1)
                        MiddleLogicIndexEnd = cell.LogicIndex;

                    visibleFillHeight += vis.Height;
                    break;
                case VisibleTypes.Ender:
                    CacheCountBottom++;
                    break;
                case VisibleTypes.Starter:
                    CacheCountTop++;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        if (visibleFillHeight > 0)
        {
            ViewPortFillPercent = visibleFillHeight / ViewPortHeight;
        }
        else
        {
            ViewPortFillPercent = 0;
        }
    }

    internal void Clear()
    {
        CacheCountTop = 0;
        CacheCountBottom = 0;
        Middle = null;
        MiddleLogicIndexStart = 0;
        MiddleLogicIndexEnd = 0;
        _cachePool.Clear();
    }

    internal void ShifleCacheCount(int topCount, int bottomCount)
    {
        CacheCountTop = topCount;
        CacheCountBottom = bottomCount;
    }

    internal void InsertCells(int startWideIndex, object[] items, SourceProvider source, Func<int, VirtualItem> funcBuildItem,
        out double rmHeight,
        out double changedScrollY)
    {
        int endWideIndex = startWideIndex + items.Length - 1;
        int cachePoolStartIndex = _cachePool.FirstOrDefault()?.LogicIndex ?? 0;
        int cachePoolEndIndex = _cachePool.LastOrDefault()?.LogicIndex ?? 0;

        RecalcCache();

        if (ViewPortFillPercent < 0.99999)
        {
            cachePoolEndIndex++;
        }

        // Выше viewport
        if (endWideIndex < cachePoolStartIndex)
        {
            double add = AvgCellHeight * items.Length;
            rmHeight = add;
            changedScrollY = add;
        }
        // Ниже viewport
        else if (startWideIndex > cachePoolEndIndex)
        {
            double add = AvgCellHeight * items.Length;
            rmHeight = add;
            changedScrollY = 0;
        }
        // Остальное, что касается viewport
        else 
        {
            int wideI = cachePoolStartIndex;
            int i = 0;
            int countNewItems = 0;
            bool usemod = false;
            double offsetY = _cachePool.FirstOrDefault()?.OffsetY ?? 0;
            double fillH = 0;

            changedScrollY = 0;
            rmHeight = 0;

            while (true)
            {
                var cell = (_cachePool.Count - 1 >= i) ? _cachePool[i] : null;
                bool isNewItem = (startWideIndex <= wideI && wideI <= endWideIndex);

                if (isNewItem || usemod)
                {
                    usemod = true;
                    if (cell == null)
                    {
                        cell = funcBuildItem(wideI);
                        cell.OffsetY = offsetY;
                        _cachePool.Add(cell);
                    }
                    else
                    {
                        cell.Shift(wideI, source);
                    }

                    cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);

                    if (isNewItem)
                    {
                        countNewItems++;
                        rmHeight += cell.DrawedSize.Height;
                    }
                }

                offsetY += cell.DrawedSize.Height;
                fillH += cell.DrawedSize.Height;

                if (wideI == source.Count - 1 || fillH >= ViewPortHeight)
                    break;

                wideI++;
                i++;
            }

            int backroomAddedItems = items.Length - countNewItems;
            rmHeight += backroomAddedItems * AvgCellHeight;
        }
    }

    internal RemoveCellsResult RemoveCells(int startWideIndex, int[] deletedIndexes, SourceProvider source, double bodyHeight, double currentScrollY)
    {
        int count = deletedIndexes.Length;
        int endWideIndex = startWideIndex + count - 1;
        int cachePoolStartIndex = _cachePool.FirstOrDefault()?.LogicIndex ?? 0;
        int cachePoolEndIndex = _cachePool.LastOrDefault()?.LogicIndex ?? 0;

        // 1. Выше viewport
        if (endWideIndex < cachePoolStartIndex)
        {
            double rm = AvgCellHeight * count;
            return new RemoveCellsResult
            {
                DeleteItems = [],
                MustBeRedraw = false,
                NewBodyHeight = bodyHeight - rm,
                NewScrollY = currentScrollY - rm,
            };
        }
        // 2. Ниже viewport
        else if (startWideIndex > cachePoolEndIndex)
        {
            double rm = AvgCellHeight * count;
            return new RemoveCellsResult
            {
                DeleteItems = [],
                MustBeRedraw = false,
                NewBodyHeight = bodyHeight - rm,
                NewScrollY = null,
            };
        }
        // 3. Остальное, что касается viewport
        else
        {
            int startIndexPool = -1;
            int startIndexLogic = -1;
            double offsetY = 0;
            int shiftCount = 0;
            bool canShift = (startWideIndex + deletedIndexes.Length - 1) < source.Count - 1;

            for (int i = 0; i < _cachePool.Count; i++)
            {
                var cell = _cachePool[i];
                if (startWideIndex <= cell.LogicIndex && cell.LogicIndex <= endWideIndex)
                {
                    if (startIndexPool == -1)
                    {
                        offsetY = cell.OffsetY;
                        startIndexPool = i;
                        startIndexLogic = cell.LogicIndex;
                    }

                    shiftCount++;
                }
            }

            if (startIndexPool < 0 || startIndexLogic < 0)
                throw new InvalidOperationException();

            var removeItems = new List<VirtualItem>();

            // 3.1 шифтим элементы сверху вниз (самый простой алгоритм)
            if (canShift)
            {
                int iterator2 = 0;
                double rmHeight = 0;
                for (int i = startIndexPool; i < _cachePool.Count; i++)
                {
                    if (removeItems.Count >= shiftCount)
                        break;

                    var cell = _cachePool[i];
                    rmHeight += cell.DrawedSize.Height;
                    int shiftIndex = startIndexLogic + iterator2;
                    if (shiftIndex > source.Count - 1)
                        Debugger.Break();

                    cell.Shift(shiftIndex, source);
                    cell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                    cell.OffsetY = offsetY;

                    offsetY += cell.DrawedSize.Height;
                    iterator2++;
                }
                return new RemoveCellsResult
                {
                    DeleteItems = [],
                    MustBeRedraw = true,
                    NewBodyHeight = bodyHeight - rmHeight,
                    NewScrollY = currentScrollY - rmHeight,
                };
            }
            // 3.2 шифт наверх
            else
            {
                double rmHeight = 0;
                VirtualItem? lastItem = null;

                // отчищаем viewport
                for (int i = _cachePool.Count - 1; i >= 0; i--)
                {
                    var cell = _cachePool[i];
                    if (startWideIndex <= cell.LogicIndex && cell.LogicIndex <= endWideIndex)
                    {
                        rmHeight += cell.DrawedSize.Height;
                    }
                    else
                    {
                        lastItem ??= cell;
                    }

                    //if (cell.LogicIndex == source.Count + deletedIndexes.Length - 1)
                    //    lastItem = cell;

                    _cachePool.Remove(cell);
                    removeItems.Add(cell);
                }

                // удаляем через задницу "неудаленные" элементы (которые были за viewport)
                rmHeight += (deletedIndexes.Length - shiftCount) * AvgCellHeight;

                double regoffset;
                double newScrollY;

                // если последний элемент найден во viewport'е,
                // то пусть он там и остается, но поднимаем его наверх, на высоту
                // удаленных элементов
                if (lastItem != null)
                {
                    regoffset = lastItem.OffsetY - rmHeight;
                    newScrollY = (regoffset + lastItem.DrawedSize.Height) - ViewPortHeight;
                }
                // Если не осталось элементов
                else if (source.Count == 0)
                {
                    regoffset = 0;
                    newScrollY = 0;
                }
                // если последний, уже отрисованный элемент не найден
                // то "насильно создаем" последний элемент в конец тела прокрутки
                else
                {
                    lastItem = removeItems.First();
                    lastItem.Shift(source.Count - 1, source);
                    lastItem.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                    regoffset = bodyHeight - rmHeight - lastItem.DrawedSize.Height;
                    newScrollY = (regoffset + lastItem.DrawedSize.Height) - ViewPortHeight;
                }

                int reglogicindex = source.Count - 1;
                double freeh = ViewPortHeight;
                while (true)
                {
                    if (reglogicindex < 0)
                        break;

                    VirtualItem reg;
                    if (removeItems.Count > 0)
                    {
                        reg = removeItems.First();
                        removeItems.Remove(reg);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    reg.OffsetY = regoffset;
                    reg.Shift(reglogicindex, source);
                    reg.HardMeasure(ViewPortWidth, double.PositiveInfinity);

                    _cachePool.Insert(0, reg);
                    reglogicindex--;
                    regoffset -= reg.DrawedSize.Height;
                    freeh -= reg.DrawedSize.Height;

                    if (freeh <= 0)
                        break;
                }

                return new RemoveCellsResult
                {
                    DeleteItems = removeItems.ToArray(),
                    MustBeRedraw = true,
                    NewBodyHeight = bodyHeight - rmHeight,
                    NewScrollY = newScrollY,
                };
            }
        }
    }

    internal double OutTopOffsetY()
    {
        if (_cachePool.Count == 0)
            return 0;

        double outOffsetY = 0;
        foreach (var item in _cachePool)
            if (item.OffsetY < 0)
                outOffsetY += Math.Abs(item.OffsetY);
            else
                break;

        return outOffsetY;
    }

    internal double OutBottomOffsetY(double estimateHeight)
    {
        if (_cachePool.Count == 0 || estimateHeight == 0)
            return 0;

        var item = _cachePool.Last();
        double delta = item.BottomLim - estimateHeight;
        if (delta > 0)
            return delta;

        return 0;
    }

    internal DirectionType GetDirection(double bodyFullHeight)
    {
        if (ScrollBottom <= bodyFullHeight)
            return DirectionType.Down;

        double perc = ViewPortHeight / bodyFullHeight;
        if (perc > 0.8 && ScrollBottom.IsEquals(bodyFullHeight, 5))
            return DirectionType.Up;

        return DirectionType.Down;
    }

    private VirtualItem? FetchCacheItem()
    {
        if (CacheCount == 0)
            return null;

        VirtualItem fetch;
        int delta = CacheCountTop - CacheCountBottom;
        if (delta > 0)
        {
            // берем из верха
            fetch = _cachePool.First();
            CacheCountTop--;
        }
        else if (delta < 0)
        {
            // берем из низа
            fetch = _cachePool.Last();
            CacheCountBottom--;
        }
        else
        {
            // берем на рандоме
            int i = _random.Next(0, 99);
            if (i < 50)
            {
                fetch = _cachePool.First();
                CacheCountTop--;
            }
            else
            {
                fetch = _cachePool.Last();
                CacheCountBottom--;
            }
        }

        _cachePool.Remove(fetch);
        return fetch;
    }

    private readonly TubeList<double> _cachesAvgHeaders = new(50);
    private readonly TubeList<double> _cachesAvgItems = new(50);
    private readonly TubeList<double> _cachesAvgFooters = new(50);

    internal double CalcAverageCellHeight(SourceProvider source)
    {
        if (_cachePool.Count == 0)
        {
            AvgCellHeight = -1;
            return AvgCellHeight;
        }

        foreach (var item in _cachePool)
        {
            switch (item.TemplateType)
            {
                case TemplateItemType.Header:
                    _cachesAvgHeaders.Add(item.DrawedSize.Height);
                    break;
                case TemplateItemType.Item:
                    _cachesAvgItems.Add(item.DrawedSize.Height);
                    break;
                case TemplateItemType.Footer:
                    _cachesAvgFooters.Add(item.DrawedSize.Height);
                    break;
                default:
                    break;
            }
        }

        var lst = new List<double>();

        if (_cachesAvgItems.Count > 0)
            lst.Add(_cachesAvgItems.Average());

        if (_cachesAvgHeaders.Count > 0)
            lst.Add(_cachesAvgHeaders.Average());

        if (_cachesAvgFooters.Count > 0)
            lst.Add(_cachesAvgFooters.Average());

        double result = 0;
        if (lst.Count > 0)
            result = lst.Average();

        AvgCellHeight = result;
        return result;

        //double fullHeaders = source.CountHeaders * avgH;
        //double fullItems = source.CountJustItems * avgI;
        //double fullFooters = source.CountFooters * avgF;

        //double abs = fullHeaders + fullItems + fullFooters;
        //double res = abs / source.Count;
        //return res;
    }

    internal void FixOffsetY(double addOffsetY)
    {
        foreach (var item in _cachePool)
        {
            item.OffsetY += addOffsetY;
        }
    }
}