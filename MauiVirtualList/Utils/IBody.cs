using MauiVirtualList.Controls;

namespace MauiVirtualList.Utils;

internal interface IBody
{
    void InvalidateVirtualCell(VirtualItem item, double deltaHeight);
}
