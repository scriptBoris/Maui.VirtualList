using Sample.Utils;
using System.Diagnostics;

namespace Sample.Models;

[DebuggerDisplay("{Name}")]
public class ServiceItem : BaseNotify
{
    public required string Name { get; set; }
    public required int DurationOnMinutes { get; set; }
    public required decimal Price { get; set; }

    public override string ToString()
    {
        return $"item_{Name}";
    }
}
