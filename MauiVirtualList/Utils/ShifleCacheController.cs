using System.Collections;
using System.Diagnostics;
using MauiVirtualList.Controls;
using MauiVirtualList.Enums;

namespace MauiVirtualList.Utils;

internal class ShifleCacheController
{
    internal ShifleCacheRules Rule {  get; set; }
    internal double ScrollTop { get; set; }
    internal double ScrollBottom { get; set; }

    internal bool Shifle2(CacheController cachePool, SourceProvider itemsSource, ref int unsolvedCacheCount)
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
            first.HardMeasure(cachePool.ViewPortWidth, double.PositiveInfinity);
            //first.Content.BindingContext = itemsSource[newIndex];
            first.OffsetY = pre.BottomLim + first.HardMeasure(cachePool.ViewPortWidth, double.PositiveInfinity).Height;

            // TODO Возможно это лишнее действие
            first.CachedPercentVis = CalcMethods.CalcVisiblePercent(first.OffsetY, first.BottomLim, ScrollTop, ScrollBottom).Percent;
            if (first.CachedPercentVis > 0)
                Debugger.Break();

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
            last.HardMeasure(cachePool.ViewPortWidth, double.PositiveInfinity);
            //last.Content.BindingContext = itemsSource[last.LogicIndex];
            last.OffsetY = pre.OffsetY - last.HardMeasure(cachePool.ViewPortWidth, double.PositiveInfinity).Height;

            // TODO Возможно это лишнее действие
            last.CachedPercentVis = CalcMethods.CalcVisiblePercent(last.OffsetY, last.BottomLim, ScrollTop, ScrollBottom).Percent;
            if (last.CachedPercentVis > 0)
                Debugger.Break();

            cachePool.Insert(0, last);
            unsolvedCacheCount--;
            cachePool.ShifleCacheCount(topCacheCount + 1, bottomCacheCount - 1);
        }

        return false;
    }
}
