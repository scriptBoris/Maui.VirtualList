using Maui.CrossPlatform.VirtualList.Enums;
using System.Diagnostics;

namespace Maui.CrossPlatform.VirtualList.Structs;

[DebuggerDisplay("Vis: {Percent}; VisibleType: {VisibleTypeString};")]
internal readonly struct OutResult
{
    public OutResult(double percent, double visHeight)
    {
        Percent = percent;
        Height = visHeight;
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

    /// <summary>
    /// Видимая высота элемента
    /// </summary>
    public double Height { get; init; }

    public VisibleTypes VisibleType { get; init; }

#if DEBUG
    public string VisibleTypeString => VisibleType.ToString();
#endif
}
