using Sample.Models;
using Sample.Utils;
using System.Collections.ObjectModel;

namespace Sample.Pages;

public partial class PageGroupList
{
    public PageGroupList(int count)
    {
        InitializeComponent();
        var groups = new ObservableCollection<ItemGroup>
        {
            new("Group", 1)
            {
                ItemTest.Gen(1),
                ItemTest.Gen(2),
                ItemTest.Gen(3),
                ItemTest.Gen(4),
            },
            new("Group", 2)
            {
                ItemTest.Gen(1),
                ItemTest.Gen(2),
                ItemTest.Gen(3),
                ItemTest.Gen(4),
                ItemTest.Gen(5),
            },
            new("Group", 3)
            {
                ItemTest.Gen(1),
                ItemTest.Gen(2),
                ItemTest.Gen(3),
            },
            new("Group", 4)
            {
                ItemTest.Gen(1),
                ItemTest.Gen(2),
            },
            new("Group", 5)
            {
                ItemTest.Gen(1),
                ItemTest.Gen(2),
                ItemTest.Gen(3),
                ItemTest.Gen(4),
                ItemTest.Gen(5),
                ItemTest.Gen(6),
                ItemTest.Gen(7),
                ItemTest.Gen(8),
                ItemTest.Gen(9),
                ItemTest.Gen(10),
            },
            new("Group", 6)
            {
                ItemTest.Gen(1),
                ItemTest.Gen(2),
            },
            new("Group", 7)
            {
                ItemTest.Gen(1),
                ItemTest.Gen(2),
                ItemTest.Gen(3),
            },
            new("Group", 8)
            {
                ItemTest.Gen(1),
                ItemTest.Gen(2),
                ItemTest.Gen(3),
            },
            new("Group", 9)
            {
                new ItemTest
                {
                    Number = 1,
                    Text = "Single Item of GROUP9"
                },
            },
            new("Group", 10)
            {
                ItemTest.Gen(1),
                ItemTest.Gen(2),
                ItemTest.Gen(3),
                ItemTest.Gen(4),
                ItemTest.Gen(5),
            }
        };

        list.ItemsSource = groups;
        Groups = groups;
    }

    public ObservableCollection<ItemGroup> Groups { get; private set; }

    private void ToolbarItem_Scroll200(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, list.ScrollY + 200, false);
    }

    private void ToolbarItem_Scroll10(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, list.ScrollY + 10, false);
    }

    private void ToolbarItem_ScrollToEnd(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, list.ContentSize.Height, false);
    }

    private void ToolbarItem_ScrollToStart(object sender, EventArgs e)
    {
        list.ScrollToAsync(0, 0, false);
    }

    private async void ToolbarItem_NewItem(object sender, EventArgs e)
    {
        //var res = await this.DisplayPromptAsync("new item", "Typing index of insert", 
        //    keyboard: Keyboard.Numeric,
        //    placeholder: "-1 (as Add)");
        //if (res == null)
        //    return;

        //int parse = -1;

        //if (!string.IsNullOrEmpty(res))
        //{
        //    if (!int.TryParse(res, out parse))
        //    {
        //        await this.DisplayAlert("Error", "Bad input data", "OK");
        //        return;
        //    }
        //}

        //int insert = parse;
        //if (parse == -1 || parse > Items.Count)
        //    insert = Items.Count;

        //Items.Insert(insert, new ItemTest
        //{
        //    Text = "NEW ITEM",
        //    Number = insert + 1,
        //});

        //for (int i = insert; i < Items.Count; i++)
        //{
        //    Items[i].Number = i + 1;
        //}

        //await list.ScrollToAsync(insert, animate: true);
    }

    private async void ToolbarItem_RemoveGroup(object sender, EventArgs e)
    {
        var res = await this.DisplayPromptAsync("remove group", "Typing number of remove",
            keyboard: Keyboard.Numeric,
            placeholder: "-1 or 0 (as last)");
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

        int removeNumber = parse;
        if (parse <= 0 || parse > Groups.Count)
            removeNumber = Groups.Count;

        Groups.RemoveAt(removeNumber - 1);

        for (int i = 0; i < Groups.Count; i++)
        {
            Groups[i].Number = i + 1;
        }
    }

    private void ToolbarItem_ClearItems(object sender, EventArgs e)
    {
        Groups.Clear();
    }
}
