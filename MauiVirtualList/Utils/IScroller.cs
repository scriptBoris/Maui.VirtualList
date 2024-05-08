namespace MauiVirtualList.Utils;

internal interface IScroller
{
    double ScrollerWidth { get; }
    double ScrollY { get; }
    double ViewPortWidth { get; }
    double ViewPortHeight { get; }
    void SetScrollY(double setupScrollY);
}