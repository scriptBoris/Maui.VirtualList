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

    internal VirtualItem? RemoveCell(int logicIndex, SourceProvider itemssource, BodyGroup body)
    {
        var exists = _cachePool.FirstOrDefault(x => x.LogicIndex == logicIndex);
        if (exists != null)
        {
            int poolIndex = _cachePool.IndexOf(exists);
            double h = exists.DrawedSize.Height;

            _cachePool.Remove(exists);
            for (int i = poolIndex; i < _cachePool.Count; i++)
            {
                var cell = _cachePool[i];
                cell.Shift(cell.LogicIndex - 1, itemssource);
                cell.Measure(ViewPortWidth, double.PositiveInfinity);
                cell.OffsetY -= h;
            }
            return exists;
        }

        return null;
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

        double avgH = _cachesAvgHeaders.Count > 0 ? _cachesAvgHeaders.Average() : 0;
        double avgI = _cachesAvgItems.Count > 0 ? _cachesAvgItems.Average() : 0;
        double avgF = _cachesAvgFooters.Count > 0 ? _cachesAvgFooters.Average() : 0;

        double fullHeaders = source.CountHeaders * avgH;
        double fullItems = source.CountJustItems * avgI;
        double fullFooters = source.CountFooters * avgF;

        double abs = fullHeaders + fullItems + fullFooters;
        double res = abs / source.Count;
        return res;
    }
}