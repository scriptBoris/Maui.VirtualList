using VirtualList.Maui.Controls;

namespace VirtualList.Maui.Utils;

internal interface IBody
{
    void InvalidateVirtualCell(VirtualItem item, double deltaHeight);
}
