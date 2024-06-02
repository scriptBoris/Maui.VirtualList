namespace VirtualList.Maui.Args;

public class ScrollItemRequest
{
    public required object Item { get; set; }
    public bool UseAnimation { get; set; }
    public ScrollToPosition ScrollToPosition { get; set; }
}
