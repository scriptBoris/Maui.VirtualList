using MauiVirtualList.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace MauiVirtualList.Utils;

internal class SourceProvider : IDisposable
{
    private readonly bool _isGroups;
    private readonly IList _source;
    private readonly IEnumerable<IEnumerable> _sourceAsGroups = null!;
    private readonly List<DoubleItem> _allItems = null!;
    private readonly List<Header> _headers = null!;
    private readonly List<Footer>? _footers;
    private byte _recalcCache = 0;

    public SourceProvider(IList source, bool useHeaders, bool useFooters)
    {
        _source = source;

        if (source is IEnumerable<IEnumerable> groups)
        {
            _isGroups = true;
            _sourceAsGroups = groups;
            _allItems = [];
            _headers = [];

            if (useFooters)
                _footers = [];

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

    #region props
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

    public bool IsGroups => _isGroups;
    public bool UsedHeaders { get; private set; }
    public bool UsedFooters { get; private set; }
    public int CountJustItems { get; private set; }
    public int CountHeaders { get; private set; }
    public int CountFooters { get; private set; }
    #endregion props

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
        _headers.Clear();
        _footers?.Clear();
        _recalcCache = newCache;
        CountHeaders = 0;
        CountJustItems = 0;
        CountFooters = 0;
        UsedHeaders = useHeaders;
        UsedFooters = useFooters;

        int itemIndex = 0;
        foreach (var group in _sourceAsGroups)
        {
            // header
            if (useHeaders)
            {
                _allItems.Add(new DoubleItem(DoubleTypes.Header, group, itemIndex));
                _headers.Add(new Header
                {
                    Context = group,
                    WideIndex = itemIndex,
                });
                CountHeaders++;
                itemIndex++;
            }
            else
            {
                _headers.Add(new Header
                {
                    Context = group,
                    WideIndex = itemIndex,
                });
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
                _footers.Add(new Footer
                {
                    Context = group,
                    WideIndex = itemIndex,
                });
                CountFooters++;
                itemIndex++;
            }
        }
    }

    internal int GetWideIndexOfHead(int logicGroupIndex)
    {
        var h = _headers[logicGroupIndex];
        return h.WideIndex;
    }

    internal void NotifySource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
                {
                    int rmIndex = e.OldStartingIndex;
                    var rmItem = (IList)e.OldItems![0]!;
                    if (rmItem is INotifyCollectionChanged rmItemNC)
                        rmItemNC.CollectionChanged -= InnerGroup_CollectionChanged;

                    int wideIndex = GetWideIndexOfHead(rmIndex);
                    int rmCount = rmItem.Count;
                    if (UsedHeaders)
                        rmCount++;
                    if (UsedFooters)
                        rmCount++;

                    _allItems.RemoveRange(wideIndex, rmCount);
                    _headers.RemoveAt(rmIndex);
                    _footers?.RemoveAt(rmIndex);
                }
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
        public required int WideIndex { get; set; }
        public required object Context { get; set; }
    }

    private class Footer
    {
        public required int WideIndex { get; set; }
        public required object Context { get; set; }
    }

    [DebuggerDisplay("{Index} | {Context}")]
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
