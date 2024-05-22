namespace MauiVirtualList.Utils;

internal interface IScroller
{
    double MeasureViewPortWidth { get; }
    double MeasureViewPortHeight { get; }
    double ScrollY { get; }
    double ViewPortWidth { get; }
    double ViewPortHeight { get; }

    void SetScrollY(double setupScrollY);
}