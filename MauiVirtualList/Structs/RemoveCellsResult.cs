﻿using MauiVirtualList.Controls;

namespace MauiVirtualList.Structs;

internal class RemoveCellsResult
{
    public required VirtualItem[] DeleteItems { get; set; }
    public required double? NewScrollY { get; set; }
    public required double? NewBodyHeight { get; set; }
}
