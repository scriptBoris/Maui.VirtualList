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
            items.Add(new ItemTest
            {
                Text = "TEST ITEM",
                Number = i + 1,
            });
        }
        list.ItemsSource = items;
        Items = items;
    }

    public ObservableCollection<ItemTest> Items { get; private set; }

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, list.ScrollY + 200, false);
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, list.ScrollY + 10, false);
    }

    private void ToolbarItem_Clicked_2(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, list.ContentSize.Height, false);
    }

    private void ToolbarItem_Clicked_3(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, 0, false);
    }

    private async void ToolbarItem_Clicked_4(object sender, EventArgs e)
    {
        var res = await this.DisplayPromptAsync("new item", "Typing index of insert", 
            keyboard: Keyboard.Numeric,
            placeholder: "-1 (as Add)");
        if (res == null)
            return;

        int parse = -1;

        if (!string.IsNullOrEmpty(res))
        {
            if (!int.TryParse(res, out parse))
            {
                await this.DisplayAlert("Error", "Bad input data", "OK");
                return;
            }
        }

        int insert = parse;
        if (parse == -1 || parse > Items.Count)
            insert = Items.Count;

        Items.Insert(insert, new ItemTest
        {
            Text = "NEW ITEM",
            Number = insert + 1,
        });

        for (int i = insert; i < Items.Count; i++)
        {
            Items[i].Number = i + 1;
        }
    }

    private async void ToolbarItem_Clicked_5(object sender, EventArgs e)
    {
        var res = await this.DisplayPromptAsync("remove item", "Typing index of remove",
            keyboard: Keyboard.Numeric,
            placeholder: "-1 (as last)");
        if (res == null)
            return;

        int parse = -1;

        if (!string.IsNullOrEmpty(res))
        {
            if (!int.TryParse(res, out parse))
            {
                await this.DisplayAlert("Error", "Bad input data", "OK");
                return;
            }
        }

        int remove = parse;
        if (parse == -1 || parse > Items.Count)
            remove = Items.Count;

        Items.RemoveAt(remove);

        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].Number = i + 1;
        }
    }
}
