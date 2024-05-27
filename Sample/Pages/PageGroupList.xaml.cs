using Maui.VirtualList.Args;
using Sample.Models;
using Sample.Utils;
using System.Collections.ObjectModel;

namespace Sample.Pages;

public partial class PageGroupList
{
    public PageGroupList(int count)
    {
        InitializeComponent();
        var array = new List<ItemGroup>()
        {
            new("A", 1)
            {
                ItemTest.Gen(1, 'A'),
                ItemTest.Gen(2, 'A'),
                ItemTest.Gen(3, 'A'),
                ItemTest.Gen(4, 'A'),
            },
            new("B", 2)
            {
                ItemTest.Gen(1, 'B'),
                ItemTest.Gen(2, 'B'),
                ItemTest.Gen(3, 'B'),
                ItemTest.Gen(4, 'B'),
                ItemTest.Gen(5, 'B'),
                ItemTest.Gen(6, 'B'),
            },
            new("C", 3)
            {
                ItemTest.Gen(1, 'C'),
                ItemTest.Gen(2, 'C'),
                ItemTest.Gen(3, 'C'),
            },
            new("D", 4)
            {
                ItemTest.Gen(1, 'D'),
                ItemTest.Gen(2, 'D'),
            },
            new("E", 5)
            {
                ItemTest.Gen(1, 'E'),
                ItemTest.Gen(2, 'E'),
                ItemTest.Gen(3, 'E'),
                ItemTest.Gen(4, 'E'),
                ItemTest.Gen(5, 'E'),
                ItemTest.Gen(6, 'E'),
                ItemTest.Gen(7, 'E'),
                ItemTest.Gen(8, 'E'),
                ItemTest.Gen(9, 'E'),
                ItemTest.Gen(10, 'E'),
            },
            new("F", 6)
            {
                ItemTest.Gen(1, 'F'),
                ItemTest.Gen(2, 'F'),
            },
            new("G", 7)
            {
                ItemTest.Gen(1, 'G'),
                ItemTest.Gen(2, 'G'),
                ItemTest.Gen(3, 'G'),
            },
            new("H", 8)
            {
                ItemTest.Gen(1, 'H'),
                ItemTest.Gen(2, 'H'),
                ItemTest.Gen(3, 'H'),
            },
            new("I", 9)
            {
                ItemTest.Gen(1, 'I'),
                ItemTest.Gen(2, 'I'),
                ItemTest.Gen(3, 'I'),
                ItemTest.Gen(4, 'I'),
            },
            new("J", 10)
            {
                ItemTest.Gen(1, 'J'),
                //ItemTest.Gen(2, 'J'),
                //ItemTest.Gen(3, 'J'),
                //ItemTest.Gen(4, 'J'),
                //ItemTest.Gen(5, 'J'),
            },
        };

        var groups = new ObservableCollection<ItemGroup>();
        foreach (var group in array.Take(count))
            groups.Add(group);

        list.ItemsSource = groups;
        Groups = groups;
    }

    public ObservableCollection<ItemGroup> Groups { get; private set; }

    private void ToolbarItem_Scroll200(object sender, EventArgs e)
    {
        list.ScrollToAsync(new ScrollCoordinateRequest
        {
            Y = list.ScrollY + 200,
        });
    }

    private void ToolbarItem_Scroll10(object sender, EventArgs e)
    {
        list.ScrollToAsync(new ScrollCoordinateRequest
        {
            Y = list.ScrollY + 10,
        });
    }

    private void ToolbarItem_minus10(object sender, EventArgs e)
    {
        list.ScrollToAsync(new ScrollCoordinateRequest
        {
            Y = list.ScrollY - 10,
        });
    }

    private void ToolbarItem_ScrollToEnd(object sender, EventArgs e)
    {
        list.ScrollToAsync(new ScrollCoordinateRequest
        {
            Y = list.ContentSize.Height,
        });
    }

    private void ToolbarItem_ScrollToStart(object sender, EventArgs e)
    {
        list.ScrollToAsync(new ScrollCoordinateRequest
        {
            Y = 0,
        });
    }

    private async void ToolbarItem_NewItem(object sender, EventArgs e)
    {
        int? insertCount = await this.DisplayNumberPrompt("new group", "Typing count of items in new group", initialValue:1);
        if (insertCount == null)
            return;

        if (insertCount.Value < 0)
            insertCount = 0;

        int? insertGroup = await this.DisplayNumberPrompt("new group", "Typing index of insert", "-1 (as Add)");
        if (insertGroup == null) 
            return;

        if (insertGroup == -1 || insertGroup > Groups.Count)
            insertGroup = Groups.Count;

        var group = new ItemGroup("NEW ITEM", insertGroup.Value + 1);

        for (int i = 0; i < insertCount.Value; i++)
        {
            group.Add(new ItemTest(i + 1, $"item sub item [index_{i}]"));
        }

        Groups.Insert(insertGroup.Value, group);

        for (int i = insertGroup.Value; i < Groups.Count; i++)
        {
            Groups[i].Number = i + 1;
        }

        await list.ScrollToAsync(new ScrollItemRequest
        {
            Item = group,
        });
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
