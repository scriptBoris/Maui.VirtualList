using Sample.Models;
using System.Collections.ObjectModel;

namespace Sample.Models;

public class ServiceGroup : ObservableCollection<ServiceItem>
{
    public ServiceGroup(string groupName)
    {
        GroupName = groupName;
    }

    public string GroupName { get; set; }
}
