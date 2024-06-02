using VirtualList.Maui.Controls;
using VirtualList.Maui.Utils;
using Microsoft.Maui.Layouts;
using System.Collections;

namespace Sample.Controls;

public class SimpleList : Layout, ILayoutManager
{
    // items source
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IList),
        typeof(SimpleList),
        null,
        propertyChanged: (b, o, n) =>
        {
            if (b is SimpleList self)
                self.Update();
        }
    );
    public IList? ItemsSource
    {
        get => GetValue(ItemsSourceProperty) as IList;
        set => SetValue(ItemsSourceProperty, value);
    }

    // item template
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(SimpleList),
        null,
        propertyChanged: (b, o, n) =>
        {
            if (b is SimpleList self)
                self.Update();
        }
    );
    public DataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty) as DataTemplate;
        set => SetValue(ItemTemplateProperty, value);
    }

    private void Update()
    {
        if (ItemsSource == null || ItemsSource.Count == 0 || ItemTemplate == null)
        {
            Children.Clear();
            return;
        }

        foreach (var item in ItemsSource)
        {
            var view = (View)ItemTemplate.CreateContent();
            view.BindingContext = item;
            throw new NotImplementedException();
            //var cell = new VirtualItem(view);
            //Children.Add(cell);
        }
    }

    public Size ArrangeChildren(Rect bounds)
    {
        double y = 0;
        foreach (var item in Children)
        {
            var r = new Rect(0, y, bounds.Width, item.DesiredSize.Height);
            item.Arrange(r);

            y += r.Height;
        }
        return bounds.Size;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        double height = 0;
        foreach (var item in Children)
        {
            var size = item.Measure(widthConstraint, heightConstraint);
            height += size.Height;
        }
        return new Size(widthConstraint, height);
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }
}