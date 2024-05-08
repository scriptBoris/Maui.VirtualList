using Sample.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Sample.Models;

[DebuggerDisplay("{GroupName}")]
public class ServiceGroup : ObservableCollection<ServiceItem>
{
    public ServiceGroup(string groupName)
    {
        GroupName = groupName;
    }

    public string GroupName { get; set; }
}
