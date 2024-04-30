using System.Collections;
using MauiVirtualList.Controls;
using MauiVirtualList.Enums;

namespace MauiVirtualList.Utils;

internal class ShifleCacheController
{
    internal ShifleCacheRules Rule {  get; set; }

    internal bool Shifle(IList<VirtualItem> cachePool, IList itemsSource, double viewportWidth,
        ref int unsolvedCacheCount,
        ref int topCacheCount,
        ref int bottomCacheCount)
    {
        bool useT2B = false;
        bool useB2T = false;

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
            first.Content.BindingContext = itemsSource[newIndex];
            first.OffsetY = pre.BottomLim + first.HardMeasure(viewportWidth, double.PositiveInfinity).Height;
            cachePool.Add(first);
            unsolvedCacheCount--;
            topCacheCount--;
            bottomCacheCount++;
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
            last.Content.BindingContext = itemsSource[last.LogicIndex];
            last.OffsetY = pre.OffsetY - last.HardMeasure(viewportWidth, double.PositiveInfinity).Height;
            cachePool.Insert(0, last);
            unsolvedCacheCount--;
            bottomCacheCount--;
            topCacheCount++;
        }

        return false;
    }
}
