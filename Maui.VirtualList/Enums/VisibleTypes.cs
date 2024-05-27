namespace Maui.VirtualList.Enums;

public enum VisibleTypes
{
    /// <summary>
    /// Элемент виден на экране (даже самый малый краешек элемента)
    /// </summary>
    Visible,

    /// <summary>
    /// Элемент исчез за пределы нижней границы ViewPort'а
    /// </summary>
    Ender,

    /// <summary>
    /// Элемент исчез за пределы верхней границы ViewPort'а
    /// </summary>
    Starter,
}
