using Sample.Models;
using System.Collections.ObjectModel;

namespace Sample.Pages;

public partial class PageList
{
    public PageList()
    {
        InitializeComponent();
        var items = new ObservableCollection<ItemTest>();
        for (int i = 0; i < 50; i++)
        {
            bool isEven = i % 2 == 0;

            items.Add(new ItemTest
            {
                Color = isEven ? Colors.Black : Colors.DarkGray,
                Number = i + 1,
            });
        }
        list.ItemsSource = items;
    }

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, 200, false);
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, list.ScrollY + 10, false);
    }

    private void ToolbarItem_Clicked_2(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, list.ContentSize.Height, false);
    }
}
