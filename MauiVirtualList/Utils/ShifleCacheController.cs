using System.Collections;
using System.Diagnostics;
using MauiVirtualList.Controls;
using MauiVirtualList.Enums;

namespace MauiVirtualList.Utils;

internal class ShifleCacheController
{
    private readonly IScroller _scroller;

    public ShifleCacheController(IScroller scroller)
    {
        _scroller = scroller;
    }

    private bool Shifle(ShifleCacheRules Rule, CacheController cachePool, SourceProvider itemsSource, ref int unsolvedCacheCount)
    {
        bool useT2B = false;
        bool useB2T = false;

        int topCacheCount = cachePool.CacheCountTop;
        int bottomCacheCount = cachePool.CacheCountBottom;

        switch (Rule)
        {
            case ShifleCacheRules.Default:
                if (unsolvedCacheCount <= 1)
                    return true;

                int delta = Math.Abs(topCacheCount - bottomCacheCount);
                if (delta <= 1)
                    return true;

                useT2B = topCacheCount > bottomCacheCount;
                useB2T = bottomCacheCount > topCacheCount;
                break;
            case ShifleCacheRules.NoCacheTop:
                if (topCacheCount == 0)
                    return true;

                useT2B = true;
                break;
            case ShifleCacheRules.NoCacheBottom:
                if (bottomCacheCount == 0)
                    return true;

                useB2T = true;
                break;
        }

        // Из начала в конец
        if (useT2B)
        {
            var pre = cachePool.Last();
            int newIndex = pre.LogicIndex + 1;
            if (newIndex > itemsSource.Count - 1)
                return true;

            var first = cachePool.First();
            cachePool.Remove(first);
            first.LogicIndex = newIndex;
            first.Shift(newIndex, itemsSource);
            first.HardMeasure(_scroller.ViewPortWidth, double.PositiveInfinity);
            first.OffsetY = pre.BottomLim;

            cachePool.Add(first);
            unsolvedCacheCount--;
            cachePool.ShifleCacheCount(topCacheCount - 1, bottomCacheCount + 1);
        }

        // Из конца в начало
        if (useB2T)
        {
            if (cachePool.First().LogicIndex == 0)
                return true;

            var last = cachePool.Last();
            cachePool.Remove(last);
            var pre = cachePool.First();
            last.LogicIndex = pre.LogicIndex - 1;
            last.Shift(last.LogicIndex, itemsSource);
            last.HardMeasure(_scroller.ViewPortWidth, double.PositiveInfinity);
            last.OffsetY = pre.OffsetY - last.DrawedSize.Height;

            cachePool.Insert(0, last);
            unsolvedCacheCount--;
            cachePool.ShifleCacheCount(topCacheCount + 1, bottomCacheCount - 1);
        }

        return false;
    }

    internal void UseShifle(ShifleCacheRules rule, CacheController cacheController, SourceProvider source)
    {
        int unsolvedCacheCount = cacheController.CacheCount;
        while (true)
        {
            bool isEnough = Shifle(rule, cacheController, source, ref unsolvedCacheCount);

            if (isEnough)
                break;
        }
    }
}
