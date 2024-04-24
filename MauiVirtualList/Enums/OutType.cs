namespace MauiVirtualList.Enums;

public enum OutType
{
    /// <summary>
    /// Элемент виден на экране (даже самый малый краешек элемента)
    /// </summary>
    No,

    /// <summary>
    /// Элемент исчез за пределы нижней границы ViewPort'а
    /// </summary>
    Ender,

    /// <summary>
    /// Элемент исчез за пределы верхней границы ViewPort'а
    /// </summary>
    Starter,
}
