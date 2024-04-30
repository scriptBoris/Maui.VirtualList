using MauiVirtualList.Utils;
using Microsoft.Maui.Layouts;
using System.Collections;
using System.Diagnostics;

namespace MauiVirtualList.Controls;

[DebuggerDisplay("Index: {LogicIndex}; OffsetY: {OffsetY}; Vis: {CachedPercentVis}")]
public class VirtualItem : Layout, ILayoutManager
{
    public VirtualItem(View content)
    {
        Content = content;
        Children.Add(content);
    }

    public View Content { get; init; }

    /// <summary>
    /// Верхний порог
    /// </summary>
    public double OffsetY { get; set; }

    /// <summary>
    /// Нижний порог
    /// </summary>
    public double BottomLim => OffsetY + DrawedSize.Height;

    /// <summary>
    /// Индекс из ItemsSource[I] (BindingContext) который содержится в 
    /// данном VirtualItem
    /// </summary>
    public int LogicIndex { get; set; }

    public bool IsCacheTop { get; set; }
    public bool IsCacheBottom { get; set; }
    public bool IsCache => IsCacheTop || IsCacheBottom;

    public Size DrawedSize => DesiredSize;

    public string DBGINFO => Content.BindingContext.ToString()!;

    public double CachedPercentVis { get; set; } = -1;

    public Size ArrangeChildren(Rect bounds)
    {
        var res = Content.HardArrange(bounds);
        //Debug.WriteLine($"ITEM ARRANGE CHILDREN: {res}");
        return res;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        var size = Content.HardMeasure(widthConstraint, heightConstraint);
        DesiredSize = size;
        //Debug.WriteLine($"ITEM MEASURE: {size}");
        return size;
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }

    internal void Shift(int newLogicalIndex, IList source)
    {
        LogicIndex = newLogicalIndex;
        Content.BindingContext = source[newLogicalIndex];
        DesiredSize = Size.Zero;
    }
}
