namespace VirtualList.Maui.Args;

public class ScrollPercentRequest
{
    /// <summary>
    /// 0.0 - 0 percent (top)<br/>
    /// 1.0 - 100 percent (bottom)
    /// </summary>
    public double PercentY { get; set; }
    public bool UseAnimation { get; set; }
}