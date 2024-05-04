﻿using Sample.Models;
using Sample.Utils;
using System.Collections.ObjectModel;

namespace Sample.Pages;

public partial class PageGroupList
{
    public PageGroupList(int count)
    {
        InitializeComponent();
        var rand = new Random();
        var groups = new ObservableCollection<ItemGroup>();
        for (int i = 0; i < count - 1; i++)
        {
            var group = new ItemGroup($"Group {i + 1}");
            var itemsCount = rand.Next(1, 10);
            for (int j = 0; j < itemsCount; j++)
            {
                group.Add(new ItemTest
                {
                    Text = $"{RandomNameGenerator.GenerateRandomMaleFirstName()} {RandomNameGenerator.GenerateRandomLastName()}",
                    Number = j + 1,
                });
            }
            groups.Add(group);
        }

        var lastGroup = new ItemGroup($"Group {count} [last group]");
        for (int j = 0; j < 5; j++)
        {
            lastGroup.Add(new ItemTest
            {
                Text = $"{RandomNameGenerator.GenerateRandomMaleFirstName()} {RandomNameGenerator.GenerateRandomLastName()}",
                Number = j + 1,
            });
        }
        groups.Add(lastGroup);

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

    private async void ToolbarItem_RemoveItem(object sender, EventArgs e)
    {
        //var res = await this.DisplayPromptAsync("remove item", "Typing index of remove",
        //    keyboard: Keyboard.Numeric,
        //    placeholder: "-1 (as last)");
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

        //int remove = parse;
        //if (parse == -1 || parse > Items.Count)
        //    remove = Items.Count - 1;

        //Items.RemoveAt(remove);

        //for (int i = 0; i < Items.Count; i++)
        //{
        //    Items[i].Number = i + 1;
        //}
    }

    private void ToolbarItem_ClearItems(object sender, EventArgs e)
    {
        Groups.Clear();
    }
}