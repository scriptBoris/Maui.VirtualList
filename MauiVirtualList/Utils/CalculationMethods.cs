using MauiVirtualList.Controls;
using MauiVirtualList.Enums;

namespace MauiVirtualList.Utils;

internal static class CalculationMethods
{
    public static VisibleTypes IsOut(VirtualItem view, double yViewPort, double heightViewPort)
    {
        double viewPortBottomLimit = yViewPort + heightViewPort;

        if (view.BottomLim < yViewPort)
            return VisibleTypes.Starter;

        if (view.OffsetY > viewPortBottomLimit)
            return VisibleTypes.Ender;

        return VisibleTypes.Visible;
    }

    public static int CalcIndexByY(double y, double scrollSize, int totalItems)
    {
        if (y == 0)
        {
            return 0;
        }
        else if (y == scrollSize)
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

    public static int CountZeroes(int number)
    {
        if (number == 0)
        {
            return 1; // Если число равно нулю, то у него один ноль
        }

        int count = 0;

        while (number != 0)
        {
            // Получаем последнюю цифру числа
            int digit = number % 10;

            // Если цифра равна нулю, увеличиваем счетчик
            if (digit == 0)
            {
                count++;
            }

            // Удаляем последнюю цифру числа
            number /= 10;
        }

        return count;
    }

    public static OutResult CalculateVisiblePercentage(double elementTop, double elementBottom, double viewportTop, double viewportBottom)
    {
        if (elementBottom <= viewportTop)
            return new OutResult(VisibleTypes.Ender);

        if (elementTop >= viewportBottom)
            return new OutResult(VisibleTypes.Starter);

        // Если элемент вышел за область видимости, возвращаем 0
        //if (elementBottom <= viewportTop || elementTop >= viewportBottom)
        //    return 0;

        // Пересекается ли элемент с viewport'ом
        double visibleTop = Math.Max(elementTop, viewportTop);
        double visibleBottom = Math.Min(elementBottom, viewportBottom);

        // Вычисляем высоту пересечения
        double visibleHeight = visibleBottom - visibleTop;

        // Вычисляем высоту элемента
        double elementHeight = elementBottom - elementTop;

        // Вычисляем процент отображения
        double visiblePercentage = (visibleHeight / elementHeight)/* * 100*/;
        return new OutResult(visiblePercentage);
    }
}

internal readonly struct OutResult
{
    public OutResult(double percent)
    {
        Percent = percent;
    }

    public OutResult(VisibleTypes outType)
    {
        VisibleType = outType;
    }

    /// <summary>
    /// 0.0 - 1.0 %
    /// </summary>
    public double Percent { get; init; }
    public VisibleTypes VisibleType { get; init; }
}