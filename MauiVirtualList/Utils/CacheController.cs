using System.Collections;
using MauiVirtualList.Controls;
using MauiVirtualList.Enums;

namespace MauiVirtualList.Utils;

internal class CacheController
{
    private readonly List<VirtualItem> _cachePool = [];
    private readonly Random _random = new();

    [Obsolete("Эксклюзивный доступ к коллекции на прямую, в будущем придумать так, " +
        "чтобы доступа из-вне небыло вообще")]
    public List<VirtualItem> ExclusiveCachePool => _cachePool;

    public double ViewPortWidth { get; private set; }
    public double ViewPortHeight { get; private set; }
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
        MiddleLogicIndexStart = 0;
        MiddleLogicIndexEnd = 0;

        // find top cache
        for (int i = 0; i < _cachePool.Count; i++)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, ScrollTop, ScrollBottom);
            cell.CachedPercentVis = vis.Percent;
            if (vis.Percent == 0)
            {
                CacheCountTop++;
            }
            else
            {
                MiddleLogicIndexStart = cell.LogicIndex;
                break;
            }
        }

        // find bottom cache
        for (int i = _cachePool.Count - 1; i >= 0; i--)
        {
            var cell = _cachePool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, ScrollTop, ScrollBottom);
            cell.CachedPercentVis = vis.Percent;
            if (vis.Percent == 0)
            {
                CacheCountBottom++;
            }
            else
            {
                MiddleLogicIndexEnd = cell.LogicIndex;
                break;
            }
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

    internal double InsertCell(int logicIndex, SourceProvider itemssource, BodyGroup body)
    {
        bool isVisible = false;
        int insertPoolIndex = 0;
        var exists = _cachePool.FirstOrDefault(x => x.LogicIndex == logicIndex);
        if (exists == null)
        {
            if (logicIndex == (itemssource.Count - 1) && body.IsScrolledToEnd)
            {
                var pre = _cachePool.Last();
                var insertCell = body.BuildCell(logicIndex, -1);
                insertCell.Measure(ViewPortWidth, double.PositiveInfinity);
                insertCell.OffsetY = pre.BottomLim;
            }
        }
        else
        {
            isVisible = exists.CachedPercentVis > 0;
            if (isVisible)
            {
                var insertCell = FetchCacheItem();
                if (insertCell == null)
                    insertCell = body.BuildCell(logicIndex, null);
                else
                    insertCell.Shift(logicIndex, itemssource);

                insertPoolIndex = _cachePool.IndexOf(exists);
                insertCell.HardMeasure(ViewPortWidth, double.PositiveInfinity);
                _cachePool.Insert(insertPoolIndex, insertCell);

                var direction = GetDirection(body.EstimatedHeight);
                if (direction == DirectionType.Down)
                {
                    int i_start = insertPoolIndex + 1;
                    double offsetPlus = insertCell.DrawedSize.Height;
                    for (int i = i_start; i < _cachePool.Count; i++)
                    {
                        var cell = _cachePool[i];
                        double savedHeight = cell.DrawedSize.Height;
                        cell.Shift(cell.LogicIndex + 1, itemssource);
                        cell.OffsetY += offsetPlus;
                        offsetPlus = savedHeight;
                    }
                }
                else
                {
                    int i_start = insertPoolIndex - 1;
                    double offsetPlus = insertCell.DrawedSize.Height;
                    for (int i = i_start; i >= 0; i--)
                    {
                        var cell = _cachePool[i];
                        double savedHeight = cell.DrawedSize.Height;
                        cell.Shift(cell.LogicIndex + 1, itemssource);
                        cell.OffsetY += offsetPlus;
                        offsetPlus = savedHeight;
                    }
                }

                return insertCell.DrawedSize.Height;
            }
        }

        return AvgCellHeight;
    }

    private VirtualItem[] RemoveCell_internal(int wideIndex, SourceProvider source, out double rmHeight, out double changedScrollY)
    {
        var exists = _cachePool.FirstOrDefault(x => x.LogicIndex == wideIndex);
        if (exists != null)
        {
            int poolIndex = _cachePool.IndexOf(exists);
            double h = exists.DrawedSize.Height;

            _cachePool.Remove(exists);
            for (int i = poolIndex; i < _cachePool.Count; i++)
            {
                var cell = _cachePool[i];
                cell.Shift(cell.LogicIndex - 1, source);
                cell.Measure(ViewPortWidth, double.PositiveInfinity);
                cell.OffsetY -= h;
            }

            changedScrollY = 0;
            rmHeight = AvgCellHeight;
            return [exists];
        }
        else
        {
            changedScrollY = 0;
            rmHeight = AvgCellHeight;
            return [];
        }
    }

    private VirtualItem[] RemoveCells_internal(int startWideIndex, int count, SourceProvider source, out double rmHeight, out double changedScrollY)
    {
        rmHeight = 0;
        changedScrollY = 0;
        int endWideIndex = startWideIndex + count - 1;
        bool isAlgorithmUp = endWideIndex != source.Count - 1;
        var removedCells = new List<VirtualItem>();

        // Удаляем элементы сверху-вниз
        if (isAlgorithmUp)
        {
            double heightRm = 0;
            
            // кол-во элементов, которые будут удалены, но уже за пределами viewport
            int invisibleCount = count;
            
            // точка старта, куда по scroll y будут вставлены следующие элементы
            double startOffsetY = -1;
            int startOffsetLogicIndex = -1;

            double currentOffsetY = _cachePool.First().OffsetY;
            VirtualItem? topDeletedItem = null;

            for (int i = 0; i < _cachePool.Count; i++)
            {
                var cell = _cachePool[i];
                int cellindex = cell.LogicIndex;
                if (cellindex >= startWideIndex && endWideIndex >= cellindex)
                {
                    // находим верхний удаляемый элемент
                    if (topDeletedItem == null)
                        topDeletedItem = cell;

                    removedCells.Add(cell);
                    _cachePool.Remove(cell);
                    heightRm += cell.DrawedSize.Height;
                    invisibleCount--;
                    i--;
                }
                // смещаем неудаляемые "нижние" элементы вверх
                // чтобы потом под них засунуть новые элементы
                else if (topDeletedItem != null)
                {
                    double rmh = (invisibleCount * AvgCellHeight) + heightRm;
                    cell.LogicIndex -= count;
                    cell.OffsetY -= rmh;
                    startOffsetY = cell.BottomLim;
                    startOffsetLogicIndex = cell.LogicIndex + 1;
                }
            }

            if (invisibleCount < 0)
                throw new InvalidOperationException();

            if (removedCells.Count == 0)
            {
                int stl = _cachePool.First().LogicIndex;
                rmHeight = AvgCellHeight * invisibleCount;

                // Если удалили за viewport
                // выше viewport
                if (startWideIndex < stl)
                {
                    foreach (var item in _cachePool)
                    {
                        item.OffsetY -= rmHeight;
                        item.LogicIndex -= invisibleCount;
                    }
                    changedScrollY = -rmHeight;
                }
                // ниже viewport
                else
                {
                    changedScrollY = 0;
                }
                return [];
            }

            if (invisibleCount > 0)
            {
                heightRm += invisibleCount * AvgCellHeight;
            }

            if (startOffsetY < 0)
            {
                if (_cachePool.Count == 0)
                {
                    startOffsetY = currentOffsetY - (invisibleCount * AvgCellHeight);
                }
                else
                {
                    startOffsetY = _cachePool.Last().BottomLim;
                }
            }

            if (startOffsetLogicIndex < 0)
            {
                startOffsetLogicIndex = startWideIndex;
            }

            // rm cache
            int setDirectIndex = startOffsetLogicIndex;
            double offHeight = 0;
            foreach (var rmcell in removedCells)
            {
                int newLogicalIndex = setDirectIndex + count;

                _cachePool.Add(rmcell);
                rmcell.OffsetY = startOffsetY + offHeight;
                rmcell.ShiftDirect(setDirectIndex, newLogicalIndex, source);
                rmcell.HardMeasure(ViewPortWidth, double.PositiveInfinity);

                offHeight += rmcell.DrawedSize.Height;
                setDirectIndex++;
            }
            rmHeight = -heightRm;
        }
        // Удаляем элементы снизу-вверх (если была удалена последяя группа)
        else
        {
            double heightRm = 0;
            VirtualItem? start = null;
            VirtualItem last = _cachePool.First();
            int totalRemoved = 0;

            for (int i = _cachePool.Count - 1; i >= 0; i--)
            {
                var cell = _cachePool[i];
                int cellindex = cell.LogicIndex;
                if (cellindex >= startWideIndex && endWideIndex >= cellindex)
                {
                    removedCells.Insert(0, cell);
                    heightRm += cell.DrawedSize.Height;
                    _cachePool.Remove(cell);
                    totalRemoved++;
                }
                else if (i == 0)
                {
                    start = cell;
                }
            }

            double startOffsetY;
            int startOffsetIndex;

            if (start != null)
            {
                startOffsetIndex = start.LogicIndex;
                startOffsetY = start.OffsetY;
            }
            else
            {
                int invisibleCounts = count - totalRemoved;
                startOffsetIndex = last.LogicIndex - invisibleCounts;
                startOffsetY = last.OffsetY - last.DrawedSize.Height;

                // Делаем поправки на количество элементов, которых нет на экране,
                // но они всё равно были удалены
                heightRm += invisibleCounts * AvgCellHeight;
                startOffsetY -= invisibleCounts * AvgCellHeight;
            }

            int insertId = startOffsetIndex - 1;
            double offY = startOffsetY;
            for (int i = 0; i < removedCells.Count; i++)
            {
                var cell = removedCells[i];
                cell.Shift(insertId - i, source);
                cell.Measure(ViewPortWidth, double.PositiveInfinity);

                offY -= cell.DrawedSize.Height;
                cell.OffsetY = offY;
                _cachePool.Insert(0, cell);
            }

            rmHeight = -heightRm;
            changedScrollY = -heightRm;
        }

        return [];
    }

    internal VirtualItem[] RemoveCellAuto(int logicIndex, SourceProvider source, out double rmHeight, out double changedScrollY)
    {
        if (source.IsGroups)
        {
            var wideIndex = source.GetWideIndexOfHead(logicIndex);
            var wideItem = (IList)source[wideIndex]!;

            int countRemovedItems = wideItem.Count;
            int startWideIndex = wideIndex;

            if (source.UsedHeaders)
                countRemovedItems++;

            if (source.UsedFooters)
                countRemovedItems++;

            if (countRemovedItems == 1)
                return RemoveCell_internal(logicIndex, source, out rmHeight, out changedScrollY);
            else
                return RemoveCells_internal(startWideIndex, countRemovedItems, source, out rmHeight, out changedScrollY);
        }
        else
        {
            return RemoveCell_internal(logicIndex, source, out rmHeight, out changedScrollY);
        }
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
                case DoubleTypes.Header:
                    _cachesAvgHeaders.Add(item.DrawedSize.Height);
                    break;
                case DoubleTypes.Item:
                    _cachesAvgItems.Add(item.DrawedSize.Height);
                    break;
                case DoubleTypes.Footer:
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
}