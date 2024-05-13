namespace MauiVirtualList.Utils;

public static class Exensions
{
    public static void HardInvalidateArrange(this View view)
    {
        ((IView)view).InvalidateArrange();
    }

    public static void HardInvalidateMeasure(this View view)
    {
        ((IView)view).InvalidateMeasure();
    }

    public static Size HardArrange(this View view, Rect rect)
    {
        return ((IView)view).Arrange(rect);
    }

    public static Size HardMeasure(this View view, double widthConstraint, double heightConstraint)
    {
        return ((IView)view).Measure(widthConstraint, heightConstraint);
    }

    public static bool IsEquals(this double self, double value, double epsilon = 0.000001)
    {
        return Math.Abs(self - value) < epsilon;
    }

    internal static byte AsByte(this bool self)
    {
        if (self)
            return 1;
        else
            return 0;
    }

    internal static void Shift(this IList<SourceProvider.HeaderFooter> source, int startFrom, int shiftOffset)
    {
        for (int i = startFrom; i < source.Count; i++)
        {
            source[i].WideIndex += shiftOffset;
        }
    }
}
