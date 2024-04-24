using MauiVirtualList.Utils;
using Microsoft.Maui.Layouts;
using System.Diagnostics;

namespace MauiVirtualList.Controls;

//public class VirtualItem : ContentPresenter, IVisualTreeElement
[DebuggerDisplay("Index: {I} OffsetY: {OffsetY}")]
public class VirtualItem : Layout, ILayoutManager//, IVisualTreeElement
{
    private Size? _drawedSize;

    public VirtualItem(View content)
    {
        Content = content;
        Children.Add(content);
    }

    public View Content { get; init; }
    public double OffsetY { get; set; }
    public double BottomLim => OffsetY + DrawedSize.Height;
    public int I { get; set; }
    public Size DrawedSize
    {
        get => _drawedSize ?? DesiredSize;
        set => _drawedSize = value;
    }

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

    //protected override Size ArrangeOverride(Rect bounds)
    //{
    //    var r = new Rect(bounds.X, bounds.Y, bounds.Width, double.PositiveInfinity);
    //    var size = Content.HardArrange(r);
    //    return size;
    //}

    //protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    //{
    //    return Content.HardMeasure(widthConstraint, heightConstraint);
    //}

    //IVisualTreeElement? IVisualTreeElement.GetVisualParent()
    //{
    //    return Parent.Parent;
    //}

    //IReadOnlyList<IVisualTreeElement> IVisualTreeElement.GetVisualChildren()
    //{
    //    return new List<IVisualTreeElement>() { Content };
    //}

    public Size ArrangeChildren(Rect bounds)
    {
        var res = Content.HardArrange(bounds);
        Debug.WriteLine($"ITEM ARRANGE CHILDREN: {res}");
        return res;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        var size = Content.HardMeasure(widthConstraint, heightConstraint);
        DesiredSize = size;
        Debug.WriteLine($"ITEM MEASURE: {size}");
        return size;
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }
}
