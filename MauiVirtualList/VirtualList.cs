using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using MauiVirtualList.Controls;
using MauiVirtualList.Utils;

namespace MauiVirtualList;

public class VirtualList :
    ScrollViewTest
    //ScrollViewProd
{
    private readonly BodyGroup _body;
    private double _cachedMeasureWidth;
    private double _cachedMeasureHeight;

    public VirtualList()
    {
        _body = new BodyGroup(this);
        Content = _body;
        Scrolled += VirtualList_Scrolled;
    }

    #region bindable props
    public DataTemplate? ItemTemplate
    {
        get => _body.ItemTemplate;
        set => _body.ItemTemplate = value;
    }

    public DataTemplate? GroupHeaderTemplate
    {
        get => _body.GroupHeaderTemplate;
        set => _body.GroupHeaderTemplate = value;
    }

    public DataTemplate? GroupFooterTemplate
    {
        get => _body.GroupFooterTemplate;
        set => _body.GroupFooterTemplate = value;
    }

    // items source
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IList),
        typeof(VirtualList),
        null,
        propertyChanged: (b, o, n) =>
        {
            if (b is VirtualList self)
            {
                self._body.SetupItemsSource(o as IList, n as IList);
            }
        }
    );
    public IList? ItemsSource
    {
        get => GetValue(ItemsSourceProperty) as IList;
        set => SetValue(ItemsSourceProperty, value);
    }

    // items spacing
    public static readonly BindableProperty ItemsSpacingProperty = BindableProperty.Create(
        nameof(ItemsSpacing),
        typeof(double),
        typeof(VirtualList),
        0.0,
        propertyChanged: (b, o, n) =>
        {
            if (b is VirtualList self)
                self._body.ItemsSpacing = (double)n;
        }
    );
    public double ItemsSpacing
    {
        get => (double)GetValue(ItemsSpacingProperty);
        set => SetValue(ItemsSpacingProperty, value);
    }

    // empty view
    public static readonly BindableProperty EmptyViewTemplateProperty = BindableProperty.Create(
        nameof(EmptyViewTemplate),
        typeof(DataTemplate),
        typeof(VirtualList),
        null,
        propertyChanged: (b, o, n) =>
        {
            if (b is VirtualList self)
                self._body.ResolveEmptyView();
        }
    );
    public DataTemplate? EmptyViewTemplate
    {
        get => GetValue(EmptyViewTemplateProperty) as DataTemplate;
        set => SetValue(EmptyViewTemplateProperty, value);
    }
    #endregion bindable props

    /// <summary>
    /// Percent scrolling
    /// </summary>
    /// <param name="percent">0.0 - 0 percent (top)<br/>1.0 - 100 percent (bottom)</param>
    /// <param name="animated"></param>
    public Task ScrollToAsync(double percent, bool animated)
    {
        var h = _body.Height * percent;
        return base.ScrollToAsync(0, h, animated);
    }

    /// <summary>
    /// Scrolling to element
    /// </summary>
    /// <param name="item">item or group item</param>
    /// <param name="position"></param>
    /// <param name="animate"></param>
    public Task ScrollToAsync(object item, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
    {
        int index = ItemsSource?.IndexOf(item) ?? -1;
        if (index == -1)
            return Task.CompletedTask;

        double y = _body.GetYItem(index);
        return base.ScrollToAsync(0, y, animate);
    }

    public async Task ScrollToAsync(int index, int groupIndex, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
    {
        //double y = _body.GetYItem(index);
        //await Task.Delay(5);
        //await base.ScrollToAsync(0, y, animate);
        throw new NotImplementedException();
    }

    //[Obsolete("No implement", true)]
    //[EditorBrowsable(EditorBrowsableState.Never)]
    //public new Task ScrollToAsync(double x, double y, bool animated)
    //{
    //    throw new NotImplementedException();
    //}

    private void VirtualList_Scrolled(object? sender, ScrolledEventArgs e)
    {
        Debug.WriteLine($"Scrollerd! Y={e.ScrollY}");
        _body.Scrolled(e.ScrollY, ViewPortWidth, ViewPortHeight);
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        if (widthConstraint == _cachedMeasureWidth && heightConstraint == _cachedMeasureHeight)
            return new Size(ViewPortWidth, ViewPortHeight);

        var res = base.MeasureOverride(widthConstraint, heightConstraint);
        
        ViewPortWidth = widthConstraint - ScrollerWidth;
        ViewPortHeight = heightConstraint;

        _cachedMeasureWidth = widthConstraint;
        _cachedMeasureHeight = heightConstraint;

        return res;
    }
}