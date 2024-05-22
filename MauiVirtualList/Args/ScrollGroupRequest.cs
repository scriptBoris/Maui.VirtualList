using System.Collections;

namespace MauiVirtualList.Args;

public class ScrollGroupRequest
{
    public required IList Group { get; set; }
    public required object ItemInGroup { get; set; }
    public ScrollToPosition ScrollToPosition { get; set; }
    public bool UseAnimation { get; set; }
}