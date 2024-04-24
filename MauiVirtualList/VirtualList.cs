using System.Collections;
using System.Diagnostics;
using MauiVirtualList.Controls;

namespace MauiVirtualList;

public class VirtualList : ScrollView
{
    private readonly Body _body = new();

    public VirtualList()
    {
        Content = _body;
        Scrolled += VirtualList_Scrolled;
    }

    #region bindable props
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
                self._body.ItemsSource = n as IList;
                self._body.Update(false, self.Width, self.Height);
            }
        }
    );
    public IList? ItemsSource
    {
        get => GetValue(ItemsSourceProperty) as IList;
        set => SetValue(ItemsSourceProperty, value);
    }

    // item template
    //public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
    //    nameof(ItemTemplate),
    //    typeof(DataTemplate),
    //    typeof(VirtualList),
    //    null,
    //    propertyChanged: (b, o, n) =>
    //    {
    //        if (b is VirtualList self)
    //        {
    //            self._body.ItemTemplate = n as DataTemplate;
    //            self._body.Update(true, self.Width, self.Height);
    //        }
    //    }
    //);
    public DataTemplate? ItemTemplate
    {
        get => _body.ItemTemplate;
        set => _body.ItemTemplate = value;
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
    #endregion bindable props

    //[Obsolete("Please use percent system instead of pixels", true)]
    //public new Task ScrollToAsync(double x, double y, bool animated)
    //{
    //    throw new NotImplementedException();
    //}

    public Task ScrollToAsync(double percent, bool animated)
    {
        var h = _body.Height * percent;
        return base.ScrollToAsync(0, h, animated);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        _body.Update(false, width, height);
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        _body.ViewPortHeight = heightConstraint;
        _body.ViewPortWidth = widthConstraint;
        var res = base.MeasureOverride(widthConstraint, heightConstraint);
        return res;
    }

    private void VirtualList_Scrolled(object? sender, ScrolledEventArgs e)
    {
        Debug.WriteLine($"Scrollerd! Y={e.ScrollY}");
        _body.Scrolled(e.ScrollY, Width, Height);
    }
}