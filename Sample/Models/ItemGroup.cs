using System.Collections.ObjectModel;

namespace Sample.Models;

public class ItemGroup : ObservableCollection<ItemTest>
{
    public ItemGroup(string groupName)
    {
        GroupName = groupName;
    }

    public string GroupName { get; set; }
}