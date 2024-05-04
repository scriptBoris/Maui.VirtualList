using MauiVirtualList.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace MauiVirtualList.Utils;

internal class SourceProvider : IDisposable
{
    private readonly bool _isGroups;
    private readonly IList _source;
    private readonly IEnumerable<IEnumerable> _groups = null!;
    private readonly List<DoubleItem> _allItems = null!;
    private byte _recalcCache = 0;

    public SourceProvider(IList source, bool useHeaders, bool useFooters)
    {
        _source = source;

        if (source is INotifyCollectionChanged sourceNC)
            sourceNC.CollectionChanged += Source_CollectionChanged;

        if (source is IEnumerable<IEnumerable> groups)
        {
            _isGroups = true;
            _allItems = [];
            _groups = groups;

            Recalc(useHeaders, useFooters);

            foreach (var group in groups)
            {
                if (group is INotifyCollectionChanged groupNC)
                    groupNC.CollectionChanged += InnerGroup_CollectionChanged;
            }
        }
        else
        {
            CountJustItems = source.Count;
        }
    }

    public object? this[int index] 
    {
        get
        {
            if (_isGroups)
            {
                var item = _allItems[index].Context;
                return item;
            }
            else
            {
                return _source[index];
            }
        }
    }

    public int Count
    {
        get
        {
            if (_isGroups)
                return _allItems.Count;
            else
                return _source.Count;
        }
    }

    public int CountJustItems { get; private set; }
    public int CountHeaders { get; private set; }
    public int CountFooters { get; private set; }

    internal DoubleTypes GetTypeItem(int index)
    {
        if (!_isGroups)
            return DoubleTypes.Item;

        var item = _allItems[index];
        return item.Type;
    }

    internal void Recalc(bool useHeaders, bool useFooters)
    {
        byte newCache = (byte)(useHeaders.AsByte() + useFooters.AsByte());
        
        if (_recalcCache == newCache)
            return;

        _allItems.Clear();
        CountHeaders = 0;
        CountJustItems = 0;
        CountFooters = 0;
        _recalcCache = newCache;

        int itemIndex = 0;
        foreach (var group in _groups)
        {
            // header
            if (useHeaders)
            {
                _allItems.Add(new DoubleItem(DoubleTypes.Header, group, itemIndex));
                CountHeaders++;
                itemIndex++;
            }

            // items
            foreach (var item in group)
            {
                _allItems.Add(new DoubleItem(DoubleTypes.Item, item, itemIndex));
                CountJustItems++;
                itemIndex++;
            }

            // footer
            if (useFooters)
            {
                _allItems.Add(new DoubleItem(DoubleTypes.Footer, group, itemIndex));
                CountFooters++;
                itemIndex++;
            }
        }
    }

    private void Source_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_isGroups)
            return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var newItem = (IList)e.NewItems![0]!;
                int newInsert = e.NewStartingIndex;
                if (newItem is INotifyCollectionChanged newItemNC)
                    newItemNC.CollectionChanged += InnerGroup_CollectionChanged;

                break;
            case NotifyCollectionChangedAction.Remove:
                var rmItem = (IList)e.OldItems![0]!;
                if (rmItem is INotifyCollectionChanged rmItemNC)
                    rmItemNC.CollectionChanged -= InnerGroup_CollectionChanged;

                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (var item in e.OldItems)
                {
                    if (item is INotifyCollectionChanged resetItemNC)
                        resetItemNC.CollectionChanged -= InnerGroup_CollectionChanged;
                }

                _allItems.Clear();
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
            default:
                throw new NotImplementedException();
        }
    }

    private void InnerGroup_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // TODO доделать для групп
    }

    public void Dispose()
    {
        if (_source is INotifyCollectionChanged sourceNC)
        {
            sourceNC.CollectionChanged -= Source_CollectionChanged;
        }

        if (_isGroups)
        {
            foreach (var group in _source)
            {
                if (group is INotifyCollectionChanged nc)
                    nc.CollectionChanged -= InnerGroup_CollectionChanged;
            }
        }
    }

    private class Header
    {
        public int Index { get; set; }
        public required object Context { get; set; }
    }

    private class Footer
    {
        public int Index { get; set; }
        public required object Context { get; set; }
    }

    private class DoubleItem
    {
        public DoubleItem(DoubleTypes type, object context, int indexItem)
        {
            Type = type;
            Index = indexItem;
            Context = context;
        }

        public DoubleTypes Type { get; }
        public int Index { get; set; } = -1;
        public object Context { get; }
    }
}
