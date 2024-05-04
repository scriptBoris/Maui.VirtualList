using Sample.Utils;

namespace Sample.Models;

public class ServiceItem : BaseNotify
{
    public required string Name { get; set; }
    public required int DurationOnMinutes { get; set; }
    public required decimal Price { get; set; }
}
