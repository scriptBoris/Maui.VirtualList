using MauiVirtualList.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MauiVirtualList.Utils;

internal class SourceProvider : IDisposable
{
    private readonly bool _isGroups;
    private readonly IList _source;
    private readonly List<Header> _headers = null!;
    private readonly List<Footer> _footers = null!;
    private readonly List<DoubleItem> _allItems = null!;

    public SourceProvider(IList source)
    {
        _source = source;

        if (source is INotifyCollectionChanged sourceNC)
            sourceNC.CollectionChanged += Source_CollectionChanged;

        if (source is IEnumerable<IEnumerable> groups)
        {
            _isGroups = true;
            //_groups = groups;
            _headers = [];
            _allItems = [];

            int headIndex = 0;
            int itemIndex = 0;
            foreach (var group in groups)
            {
                _allItems.Add(new DoubleItem(DoubleTypes.Header, group, indexGroup:headIndex));
                _headers.Add(new Header
                {
                    Index = itemIndex,
                    Context = group,
                });
                itemIndex++;

                foreach (var item in group)
                {
                    _allItems.Add(new DoubleItem(DoubleTypes.Item, item, indexItem:itemIndex));
                    itemIndex++;
                    CountJustItems++;
                }

                if (group is INotifyCollectionChanged groupNC)
                    groupNC.CollectionChanged += InnerGroup_CollectionChanged;

                headIndex++;
                CountHeadersOrFooters++;
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
                //return item.Type switch
                //{
                //    DoubleTypes.Header => _headers[item.IndexGroup].Context,
                //    DoubleTypes.Item => _source[item.IndexItem],
                //    _ => throw new NotImplementedException(),
                //};
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
    public int CountHeadersOrFooters { get; private set; }

    internal DoubleTypes GetTypeItem(int index)
    {
        if (!_isGroups)
            return DoubleTypes.Item;

        var item = _allItems[index];
        return item.Type;
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

                _headers.Insert(newInsert, new Header
                {
                    Index = newInsert,
                    Context = newItem,
                });

                break;
            case NotifyCollectionChangedAction.Remove:
                var rmItem = (IList)e.OldItems![0]!;
                if (rmItem is INotifyCollectionChanged rmItemNC)
                    rmItemNC.CollectionChanged -= InnerGroup_CollectionChanged;

                var match = _headers[e.OldStartingIndex];
                _headers.Remove(match);
                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (var item in e.OldItems)
                {
                    if (item is INotifyCollectionChanged resetItemNC)
                        resetItemNC.CollectionChanged -= InnerGroup_CollectionChanged;
                }
                _headers.Clear();
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
        public DoubleItem(DoubleTypes type, object context, int indexGroup = -1, int indexItem = -1)
        {
            Type = type;
            IndexGroup = indexGroup;
            IndexItem = indexItem;
            Context = context;
        }

        public DoubleTypes Type { get; }
        public int IndexGroup { get; }
        public int IndexItem { get; }
        public object Context { get; }
    }
}
