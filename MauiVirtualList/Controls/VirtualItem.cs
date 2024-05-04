using MauiVirtualList.Enums;
using MauiVirtualList.Utils;
using Microsoft.Maui.Layouts;
using System.Collections;
using System.Diagnostics;

namespace MauiVirtualList.Controls;

[DebuggerDisplay("Index: {LogicIndex}; OffsetY: {OffsetY}; Vis: {CachedPercentVis}")]
public class VirtualItem : Layout, ILayoutManager
{
    private readonly Dictionary<DoubleTypes, View> _cache = [];
    private View _content;

    internal VirtualItem(View content, DoubleTypes templateType)
    {
        _content = content;
        TemplateType = templateType;
        Children.Add(content);

        _cache.Add(templateType, content);
    }

    internal DoubleTypes TemplateType { get; private set; }

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

    #if DEBUG
    public string DBGINFO => _content.BindingContext.ToString() ?? "<No data>";
    #endif
    
    public Size ArrangeChildren(Rect bounds)
    {
        _content.HardArrange(bounds);
        //Debug.WriteLine($"ITEM ARRANGE CHILDREN: {res}");
        return bounds.Size;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        var size = _content.HardMeasure(widthConstraint, heightConstraint);
        DesiredSize = size;
        AwaitRecalcMeasure = false;
        //Debug.WriteLine($"ITEM MEASURE: {size}");
        return size;
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }

    internal void Shift(int newLogicalIndex, SourceProvider source)
    {
        var context = source[newLogicalIndex];
        var templateType = source.GetTypeItem(newLogicalIndex);

        //this.BatchBegin();
        if (templateType == TemplateType)
        {
            LogicIndex = newLogicalIndex;
            _content.BindingContext = source[newLogicalIndex];
            DesiredSize = Size.Zero;
            AwaitRecalcMeasure = true;
        }
        else
        {
            var parent = (BodyGroup)Parent;
            var old = _content;
            old.BindingContext = null;
            Children.Remove(old);

            if (_cache.TryGetValue(templateType, out var vv))
            {
                _content = vv;
            }
            else
            {
                var createdView = templateType switch
                {
                    DoubleTypes.Header => parent.GroupHeaderTemplate?.CreateContent() as View,
                    DoubleTypes.Item => parent.ItemTemplate.CreateContent() as View,
                    DoubleTypes.Footer => parent.GroupFooterTemplate?.CreateContent() as View,
                    _ => throw new InvalidOperationException(),
                } ?? throw new InvalidOperationException();

                _content = createdView;
                _cache.Add(templateType, _content);
            }

            TemplateType = templateType;
            Children.Add(_content);
            LogicIndex = newLogicalIndex;
            _content.BindingContext = context;
            DesiredSize = Size.Zero;
            AwaitRecalcMeasure = true;
        }
        //this.BatchCommit();
    }
}
