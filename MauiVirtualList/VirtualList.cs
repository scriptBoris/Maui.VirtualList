using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using MauiVirtualList.Args;
using MauiVirtualList.Controls;
using MauiVirtualList.Utils;

namespace MauiVirtualList;

public class VirtualList :
    ScrollViewOut
    //ScrollViewProd
{
    private readonly BodyGroup _body;

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
    public Task ScrollToAsync(ScrollPercentRequest request)
    {
        var h = _body.Height * request.PercentY;
        return base.ScrollToAsync(0, h, request.UseAnimation);
    }

    /// <summary>
    /// Scrolling to element
    /// </summary>
    /// <param name="item">item or group item</param>
    /// <param name="position"></param>
    /// <param name="animate"></param>
    public Task ScrollToAsync(ScrollItemRequest request)
    {
        int index = ItemsSource?.IndexOf(request.Item) ?? -1;
        if (index == -1)
            return Task.CompletedTask;

        double y = _body.GetYItem(index);
        return base.ScrollToAsync(0, y, request.UseAnimation);
    }

    public Task ScrollToAsync(ScrollCoordinateRequest request)
    {
        return base.ScrollToAsync(request.X, request.Y, request.UseAnimation);
    }

    public async Task ScrollToAsync(ScrollGroupRequest request)
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
        MeasureViewPortWidth = widthConstraint;
        MeasureViewPortHeight = heightConstraint;
        var res = base.MeasureOverride(widthConstraint, heightConstraint);
        ViewPortWidth = widthConstraint;
        ViewPortHeight = heightConstraint;
        return res;
    }
}