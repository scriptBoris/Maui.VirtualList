using MauiVirtualList.Utils;

namespace MauiVirtualList.Controls;

public class ScrollViewProd : ScrollView, IScroller
{
    public double ViewPortWidth { get; set; }
    public double ViewPortHeight { get; set; }
    public double ScrollerWidth => 0;

    public void SetScrollY(double setupScrollY)
    {
    }
}
