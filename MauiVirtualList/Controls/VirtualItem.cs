using MauiVirtualList.Utils;
using Microsoft.Maui.Layouts;
using System.Diagnostics;

namespace MauiVirtualList.Controls;

[DebuggerDisplay("Index: {I} OffsetY: {OffsetY}")]
public class VirtualItem : Layout, ILayoutManager
{
    //private Size? _drawedSize;

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
    
    public int I { get; set; }

    public Size DrawedSize => DesiredSize;
    //public Size DrawedSize
    //{
    //    get => _drawedSize ?? DesiredSize;
    //    set => _drawedSize = value;
    //}

    public string DBGINFO
    {
        get
        {
            if (Content is Label lbl)
                return lbl.Text;
            else
                return "";
        }
    }

    public double CachedPercentVis { get; set; }

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
}
