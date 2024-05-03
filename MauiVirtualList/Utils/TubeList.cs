namespace MauiVirtualList.Utils;

internal class TubeList<T> : List<T>, ICollection<T>
{
    public TubeList(int size)
    {
        Size = size;
    }

    public int Size { get; private set; }

    public new void Add(T item) 
    {
        if (this.Count > Size)
            RemoveAt(0);
        base.Add(item);
    }
}
