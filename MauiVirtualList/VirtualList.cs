using System.Collections;
using System.Diagnostics;
using MauiVirtualList.Controls;

namespace MauiVirtualList;

public class VirtualList : 
    ScrollViewTest 
    //ScrollView
{
    private readonly Body _body = new();

    public VirtualList()
    {
        Content = _body;
        Scrolled += VirtualList_Scrolled;
    }

    #region bindable props
    public IList? ItemsSource
    {
        get => _body.ItemsSource;
        set => _body.ItemsSource = value;
    }
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

    public Task ScrollToAsync(double percent, bool animated)
    {
        var h = _body.Height * percent;
        return base.ScrollToAsync(0, h, animated);
    }

    private void VirtualList_Scrolled(object? sender, ScrolledEventArgs e)
    {
        Debug.WriteLine($"Scrollerd! Y={e.ScrollY}");
        _body.Scrolled(e.ScrollY, Width, Height);
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        _body.ViewPortHeight = heightConstraint;
        _body.ViewPortWidth = widthConstraint;
        var res = base.MeasureOverride(widthConstraint, heightConstraint);
        //if (res.Height> widthConstraint)
        //{
        //    res = new Size(res.Width, heightConstraint);
        //    DesiredSize = res;
        //}
        return res;
    }
}