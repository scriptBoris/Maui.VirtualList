namespace Maui.VirtualList.Utils;

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

    // У винды другое представлении о компановке элементов на экране
#if WINDOWS
    public static Size HardArrange(this View view, Rect rect)
    {
        var m = view.Margin;
        double x = rect.X - m.Left;
        double y = rect.Y - m.Top;
        var r = new Rect(x, y, rect.Width, rect.Height);
        var res = ((IView)view).Arrange(r);
        var res2 = new Size(res.Width, res.Height-m.VerticalThickness);
        return res2;
    }
#else
    public static Size HardArrange(this View view, Rect rect)
    {
        var res = ((IView)view).Arrange(rect);
        return res;
    }
#endif

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

    internal static void Shift(this IList<SourceProvider.Group> source, int startFrom, int shiftOffset)
    {
        for (int i = startFrom; i < source.Count; i++)
        {
            source[i].WideIndex += shiftOffset;
        }
    }
}
