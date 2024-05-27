using Maui.VirtualList.Controls;

namespace Maui.VirtualList.Utils;

internal interface IBody
{
    void InvalidateVirtualCell(VirtualItem item, double deltaHeight);
}
