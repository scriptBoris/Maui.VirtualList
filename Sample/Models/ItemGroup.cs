using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Sample.Models;

[DebuggerDisplay("{DEBUGINFO}")]
public class ItemGroup : ObservableCollection<ItemTest>
{
    public ItemGroup(string groupName, int number)
    {
        GroupName = groupName;
        Number = number;
        InitId = number - 1;
    }

    public string GroupName { get; set; }
    public int Number { get; set; }
    public int InitId { get; set; }

    public string DEBUGINFO => ToString();

    public override string ToString()
    {
        return $"#{Number} {GroupName} / init id:{InitId}";
    }
}