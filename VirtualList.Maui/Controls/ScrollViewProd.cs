using VirtualList.Maui.Utils;

namespace VirtualList.Maui.Controls;

public class ScrollViewOut : ScrollView, IScroller
{
    public double MeasureViewPortWidth { get; internal set; }
    public double MeasureViewPortHeight { get; internal set; }
    public double ViewPortWidth { get; internal set; }
    public double ViewPortHeight { get; internal set; }

    bool IScroller.SetScrollY(double setupScrollY)
    {
        if (setupScrollY < 0)
            setupScrollY = 0;
        else if (setupScrollY > ContentSize.Height)
            setupScrollY = ContentSize.Height;

        if (ScrollY == setupScrollY)
            return false;

        this.ScrollToAsync(0, setupScrollY, false);
        return true;
    }
}
