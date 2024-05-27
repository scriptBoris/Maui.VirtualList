using Maui.VirtualList.Controls;

namespace Maui.VirtualList.Structs;

internal class RemoveCellsResult
{
    public required VirtualItem[] DeleteItems { get; set; }
    public required double? NewScrollY { get; set; }
    public required double? NewBodyHeight { get; set; }
    public required bool MustBeRedraw { get; set; }
}
