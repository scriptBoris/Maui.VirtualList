using MauiVirtualList.Utils;

namespace MauiVirtualList.Controls;

public class ScrollViewProd : ScrollView, IScroller
{
    public double MeasureViewPortWidth { get; internal set; }
    public double MeasureViewPortHeight { get; internal set; }
    public double ViewPortWidth { get; internal set; }
    public double ViewPortHeight { get; internal set; }

    void IScroller.SetScrollY(double setupScrollY)
    {
        this.ScrollToAsync(0, setupScrollY, false);
    }
}
