using MauiVirtualList.Controls;
using MauiVirtualList.Enums;

namespace MauiVirtualList.Utils;

public static class CalculationMethods
{
    public static OutType IsOut(VirtualItem view, double yViewPort, double heightViewPort)
    {
        double viewPortBottomLimit = yViewPort + heightViewPort;

        if (view.BottomLim < yViewPort)
            return OutType.Starter;

        if (view.OffsetY > viewPortBottomLimit)
            return OutType.Ender;

        return OutType.No;
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
}
