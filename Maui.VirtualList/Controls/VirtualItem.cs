using Maui.VirtualList.Enums;
using Maui.VirtualList.Utils;
using Microsoft.Maui.Layouts;
using System.Collections;
using System.Diagnostics;

namespace Maui.VirtualList.Controls;

[DebuggerDisplay("Index: {LogicIndex}; OffsetY: {OffsetY}; Vis: {CachedPercentVis} | {DBGINFO}")]
public class VirtualItem : Layout, ILayoutManager
{
    private readonly Dictionary<TemplateItemType, View> _cache = [];
    private readonly WeakReference<IScroller> _scroller;
    private readonly WeakReference<IBody> _body;
    private View? _content;

    internal VirtualItem(View content, TemplateItemType templateType, IScroller scroller, IBody body)
    {
        _cache.Add(templateType, content);
        _scroller = new(scroller);
        _body = new(body);
        Content = content;
        TemplateType = templateType;
    }

    internal TemplateItemType TemplateType { get; private set; }

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
    public double CachedPercentVis { get; set; } = -1;

    /// <summary>
    /// Требуется ли пересчет размера для данного элемента или нет
    /// </summary>
    public bool AwaitRecalcMeasure { get; private set; } = true;

    private View Content
    {
        get => _content!;
        set
        {
            if (value == _content)
                return;

            var old = _content;
            if (old != null)
            {
                Unsubscribe(old);
                old.BindingContext = null;
                Children.Remove(old);
            }

            _content = value;

            if (value != null)
            {
                Children.Add(value);
                Subscribe(value);
            }
        }
    }

#if DEBUG
    public string DBGINFO => Content.BindingContext.ToString() ?? "<No data>";
#endif

    public Size ArrangeChildren(Rect bounds)
    {
        var res = Content.HardArrange(bounds);
        return res;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        var size = Content.HardMeasure(widthConstraint, heightConstraint);
        DesiredSize = size;
        AwaitRecalcMeasure = false;
        return size;
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }

    internal void Deactivate()
    {
        Content = null!;
    }

    internal void Shift(int newLogicalIndex, SourceProvider source)
    {
        var context = source[newLogicalIndex];
        var templateType = source.GetTypeItem(newLogicalIndex);

        //this.BatchBegin();
        if (templateType == TemplateType)
        {
            AwaitRecalcMeasure = true;
            LogicIndex = newLogicalIndex;
            Content.BindingContext = context;
            this.HardInvalidateMeasure();
            DesiredSize = Size.Zero;
        }
        else
        {
            var parent = (Body)Parent;
            View newContent;

            if (_cache.TryGetValue(templateType, out var existView))
            {
                newContent = existView;
            }
            else
            {
                var createdView = templateType switch
                {
                    TemplateItemType.Header => parent.GroupHeaderTemplate?.CreateContent() as View,
                    TemplateItemType.Item => parent.ItemTemplate.CreateContent() as View,
                    TemplateItemType.Footer => parent.GroupFooterTemplate?.CreateContent() as View,
                    _ => throw new InvalidOperationException(),
                } ?? throw new InvalidOperationException();

                newContent = createdView;
                _cache.Add(templateType, newContent);
            }

            AwaitRecalcMeasure = true;
            TemplateType = templateType;
            LogicIndex = newLogicalIndex;
            Content = newContent;
            Content.BindingContext = context;
            this.HardInvalidateMeasure();
            DesiredSize = Size.Zero;
        }
        //this.BatchCommit();
    }

    private void Unsubscribe(IVisualTreeElement view)
    {
        if (view is View v)
        {
            v.MeasureInvalidated -= SubviewMeasureInvalidated;
            v.ChildAdded -= SubviewChildAdded;
            v.ChildRemoved -= SubviewChildRemoved;
        }

        var tree = view.GetVisualChildren();
        foreach (var child in tree)
        {
            Unsubscribe(child);
        }
    }

    private void Subscribe(IVisualTreeElement view)
    {
        if (view is View v)
        {
            v.MeasureInvalidated += SubviewMeasureInvalidated;
            v.ChildAdded += SubviewChildAdded;
            v.ChildRemoved += SubviewChildRemoved;
        }

        var tree = view.GetVisualChildren();
        foreach (var child in tree)
        {
            Subscribe(child);
        }
    }

    private void SubviewChildAdded(object? sender, ElementEventArgs e)
    {
        Subscribe(e.Element);
    }

    private void SubviewChildRemoved(object? sender, ElementEventArgs e)
    {
        Unsubscribe(e.Element);
    }

    private void SubviewMeasureInvalidated(object? sender, EventArgs e)
    {
        if (!_scroller.TryGetTarget(out var scroller))
            return;

        if (!_body.TryGetTarget(out var body))
            return;

        if (AwaitRecalcMeasure)
            return;

        var oldSize = DrawedSize;
        var newSize = this.HardMeasure(scroller.ViewPortWidth, double.PositiveInfinity);
        if (newSize != oldSize)
        {
            double deltaH = newSize.Height - oldSize.Height;
            body.InvalidateVirtualCell(this, deltaH);
        }
    }
}
