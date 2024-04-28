using MauiVirtualList.Enums;
using System.Diagnostics;

namespace MauiVirtualList.Structs;

[DebuggerDisplay("Vis: {Percent}; VisibleType: {VisibleTypeString};")]
internal readonly struct OutResult
{
    public OutResult(double percent)
    {
        Percent = percent;
        VisibleType = VisibleTypes.Visible;
    }

    public OutResult(VisibleTypes outType)
    {
        VisibleType = outType;
    }

    /// <summary>
    /// 0.0 - 1.0 %
    /// </summary>
    public double Percent { get; init; }
    public VisibleTypes VisibleType { get; init; }

#if DEBUG
    public string VisibleTypeString => VisibleType.ToString();
#endif
}
