using Maui.CrossPlatform.VirtualList.Controls;

namespace Maui.CrossPlatform.VirtualList.Utils;

internal interface IBody
{
    void InvalidateVirtualCell(VirtualItem item, double deltaHeight);
}
