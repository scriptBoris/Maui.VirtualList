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
}
