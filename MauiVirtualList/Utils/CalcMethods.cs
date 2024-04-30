﻿using MauiVirtualList.Controls;
using MauiVirtualList.Enums;
using MauiVirtualList.Structs;

namespace MauiVirtualList.Utils;

internal static class CalcMethods
{
    internal static VisibleTypes IsOut(VirtualItem view, double yViewPort, double heightViewPort)
    {
        double viewPortBottomLimit = yViewPort + heightViewPort;

        if (view.BottomLim < yViewPort)
            return VisibleTypes.Starter;

        if (view.OffsetY > viewPortBottomLimit)
            return VisibleTypes.Ender;

        return VisibleTypes.Visible;
    }

    internal static int CalcIndexByY(double y, double scrollSize, int totalItems)
    {
        if (y <= 0)
        {
            return 0;
        }
        else if (y >= scrollSize)
        {
            return totalItems;
        }
        else
        {
            double per = (y / scrollSize);
            double t = (double)totalItems * per;
            int res = (int)t;
            return res;
        }
    }

    internal static double ChekHoleTop(double viewPortTop, double viewPortBottom, VirtualItem item)
    {
        if (item.BottomLim < viewPortTop)
            return 0;

        if (item.OffsetY > viewPortBottom)
            return 0;

        double topDif = viewPortTop - item.OffsetY;
        if (topDif < 0)
            return -topDif;
        else
            return 0;
    }

    internal static OutResult CalcVisiblePercent(double elementTop, double elementBottom, double viewportTop, double viewportBottom)
    {
        if (elementBottom < viewportTop)
            return new OutResult(VisibleTypes.Starter);

        if (elementTop > viewportBottom)
            return new OutResult(VisibleTypes.Ender);

        // Пересекается ли элемент с viewport'ом
        double visibleTop = Math.Max(elementTop, viewportTop);
        double visibleBottom = Math.Min(elementBottom, viewportBottom);

        // Вычисляем высоту пересечения
        double visibleHeight = visibleBottom - visibleTop;
        if (visibleHeight == 0)
            return new OutResult(VisibleTypes.Starter);

        // Вычисляем высоту элемента
        double elementHeight = elementBottom - elementTop;

        // Вычисляем процент отображения
        double visiblePercentage = (visibleHeight / elementHeight)/* * 100*/;
        return new OutResult(visiblePercentage);
    }

    internal static void RecalcCache(IList<VirtualItem> cachepool, double viewportTop, double viewportBottom,
        out int middleLogicIndexStart,
        out int middleLogicIndexEnd,
        out int cacheCount,
        out int topCacheCount,
        out int bottomCacheCount)
    {
        middleLogicIndexStart = 0;
        middleLogicIndexEnd = 0;
        cacheCount = 0; 
        topCacheCount = 0;
        bottomCacheCount = 0;
        
        // find top cache
        for (int i = 0; i < cachepool.Count; i++)
        {
            var cell = cachepool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, viewportTop, viewportBottom);
            cell.CachedPercentVis = vis.Percent;
            if (vis.Percent == 0)
            {
                cacheCount++;
                topCacheCount++;
            }
            else
            {
                middleLogicIndexStart = cell.LogicIndex;
                break;
            }
        }

        // find bottom cache
        for (int i = cachepool.Count - 1; i >= 0; i--)
        {
            var cell = cachepool[i];
            var vis = CalcMethods
                .CalcVisiblePercent(cell.OffsetY, cell.BottomLim, viewportTop, viewportBottom);
            cell.CachedPercentVis = vis.Percent;
            if (vis.Percent == 0)
            {
                cacheCount++;
                bottomCacheCount++;
            }
            else
            {
                middleLogicIndexEnd = cell.LogicIndex;
                break;
            }
        }
    }
}