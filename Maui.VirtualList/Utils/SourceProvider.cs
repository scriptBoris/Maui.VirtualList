using Maui.VirtualList.Enums;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Maui.VirtualList.Utils;

internal delegate void SourceHandler_Insert(int wideindexStart, object[] items);
internal delegate void SourceHandler_Removed(int wideindexStart, int[] deletedIndexes);
internal delegate void SourceHandler_Cleared();

internal class SourceProvider : IDisposable
{
    private readonly bool _isGroups;
    private readonly IList _source;
    private readonly IEnumerable<IEnumerable> _sourceAsGroups = null!;
    private readonly List<SourceItem> _allItems = null!;
    private readonly List<Group> _groups = null!;
    private byte _recalcCache = 0;

    public event SourceHandler_Insert? ItemsAdded;
    public event SourceHandler_Removed? ItemsRemoved;
    public event SourceHandler_Cleared? ItemsCleared;

    public SourceProvider(IList source, bool useHeaders, bool useFooters)
    {
        _source = source;

        if (source is IEnumerable<IEnumerable> groups)
        {
            _isGroups = true;
            _sourceAsGroups = groups;
            _allItems = [];
            _groups = [];

            Recalc(useHeaders, useFooters);
        }

        if (source is INotifyCollectionChanged collection)
            collection.CollectionChanged += MainCollection_CollectionChanged;
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
    #endregion props

    internal TemplateItemType GetTypeItem(int index)
    {
        if (!_isGroups)
            return TemplateItemType.Item;

        var item = _allItems[index];
        return item.Type;
    }

    internal void Recalc(bool useHeaders, bool useFooters)
    {
        byte newCache = (byte)(useHeaders.AsByte() + useFooters.AsByte());

        if (_recalcCache == newCache)
            return;

        _allItems.Clear();
        _groups.Clear();
        _recalcCache = newCache;
        UsedHeaders = useHeaders;
        UsedFooters = useFooters;

        int itemIndex = 0;
        foreach (var group in _sourceAsGroups)
        {
            _groups.Add(new Group(this, group, itemIndex, useHeaders, useFooters));

            // header
            if (useHeaders)
            {
                _allItems.Add(new SourceItem(TemplateItemType.Header, group));
                itemIndex++;
            }

            // items
            foreach (var item in group)
            {
                _allItems.Add(new SourceItem(TemplateItemType.Item, item));
                itemIndex++;
            }

            // footer
            if (useFooters)
            {
                _allItems.Add(new SourceItem(TemplateItemType.Footer, group));
                itemIndex++;
            }
        }
    }

    internal int GetWideIndexOfHead(int logicGroupIndex)
    {
        var h = _groups[logicGroupIndex];
        return h.WideIndex;
    }

    //internal void NotifySource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    //{
    //    if (!_isGroups)
    //        return;

    //    switch (e.Action)
    //    {
    //        case NotifyCollectionChangedAction.Add:
    //            var newItem = (IList)e.NewItems![0]!;
    //            int newInsert = e.NewStartingIndex;
    //            if (newItem is INotifyCollectionChanged newItemNC)
    //                newItemNC.CollectionChanged += InnerGroup_CollectionChanged;

    //            // todo реализовать добавление элементов

    //            break;
    //        case NotifyCollectionChangedAction.Remove:
    //            {
    //                int rmIndex = e.OldStartingIndex;
    //                var rmItem = (IList)e.OldItems![0]!;
    //                if (rmItem is INotifyCollectionChanged rmItemNC)
    //                    rmItemNC.CollectionChanged -= InnerGroup_CollectionChanged;

    //                int wideIndex = GetWideIndexOfHead(rmIndex);
    //                int rmCount = rmItem.Count;
    //                if (UsedHeaders)
    //                    rmCount++;
    //                if (UsedFooters)
    //                    rmCount++;

    //                _allItems.RemoveRange(wideIndex, rmCount);
    //                _groups.Shift(rmIndex, -rmCount);
    //                _groups.RemoveAt(rmIndex);

    //                _footers?.Shift(rmIndex, -rmCount);
    //                _footers?.RemoveAt(rmIndex);
    //            }
    //            break;
    //        case NotifyCollectionChangedAction.Reset:
    //            foreach (var item in _groups)
    //            {
    //                if (item.Context is INotifyCollectionChanged resetItemNC)
    //                    resetItemNC.CollectionChanged -= InnerGroup_CollectionChanged;
    //            }

    //            _allItems.Clear();
    //            _groups.Clear();
    //            break;
    //        case NotifyCollectionChangedAction.Replace:
    //        case NotifyCollectionChangedAction.Move:
    //        default:
    //            throw new NotImplementedException();
    //    }
    //}

    internal void InnerGroup_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!MainThread.IsMainThread)
            throw new InvalidOperationException("Code is not running on the main thread.");

        throw new NotImplementedException();
    }

    private void MainCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!MainThread.IsMainThread)
            throw new InvalidOperationException("Code is not running on the main thread.");

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var newGroupItem = e.NewItems![0]!;
                int newGroupIndex = e.NewStartingIndex;
                if (IsGroups)
                {
                    int wideIndex;
                    int prevIndex = newGroupIndex - 1;
                    if (prevIndex < 0)
                    {
                        wideIndex = 0;
                    }
                    else
                    {
                        var prev = _groups[prevIndex];
                        wideIndex = prev.WideIndex + prev.CountElements;
                    }

                    var newHeader = new Group(this, newGroupItem, wideIndex, UsedHeaders, UsedFooters);
                    _groups.Insert(newGroupIndex, newHeader);

                    int index = wideIndex;
                    // header
                    if (UsedHeaders)
                    {
                        _allItems.Insert(index, new SourceItem(TemplateItemType.Header, newGroupItem));
                        index++;
                    }

                    // items
                    if (newGroupItem is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            _allItems.Insert(index, new SourceItem(TemplateItemType.Item, item));
                            index++;
                        }
                    }
                    else
                    {
                        _allItems.Insert(index, new SourceItem(TemplateItemType.Item, newGroupItem));
                        index++;
                    }

                    // footer
                    if (UsedFooters)
                    {
                        _allItems.Insert(index, new SourceItem(TemplateItemType.Footer, newGroupItem));
                    }

                    // shift under groups
                    for (int i = newGroupIndex + 1; i < _groups.Count; i++)
                    {
                        var group = _groups[i];
                        group.WideIndex += newHeader.CountElements;
                    }

                    ItemsAdded?.Invoke(wideIndex, newHeader.Items);
                }
                else
                {
                    ItemsAdded?.Invoke(e.NewStartingIndex, [newGroupItem]);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (IsGroups)
                {
                    var deletedGroup = _groups[e.OldStartingIndex];
                    deletedGroup.Dispose();
                    _groups.Remove(deletedGroup);
                    _allItems.RemoveRange(deletedGroup.WideIndex, deletedGroup.CountElements);

                    // shift under groups
                    for (int i = e.OldStartingIndex; i < _groups.Count; i++)
                    {
                        var group = _groups[i];
                        group.WideIndex -= deletedGroup.CountElements;
                    }

                    int[] dels = new int[deletedGroup.CountElements];
                    for (int i = 0; i < deletedGroup.CountElements; i++)
                        dels[i] = deletedGroup.WideIndex + i;

                    ItemsRemoved?.Invoke(deletedGroup.WideIndex, dels);
                }
                else
                {
                    ItemsRemoved?.Invoke(e.OldStartingIndex, [e.OldStartingIndex]);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                throw new NotImplementedException();
            case NotifyCollectionChangedAction.Move:
                throw new NotImplementedException();
            case NotifyCollectionChangedAction.Reset:
                if (IsGroups)
                {
                    _groups.Clear();
                    _allItems.Clear();
                }
                ItemsCleared?.Invoke();
                break;
            default:
                break;
        }
    }

    public void Dispose()
    {
        if (_isGroups)
        {
            foreach (var group in _groups)
            {
                group.Dispose();
            }
        }

        if (_source is INotifyCollectionChanged collection)
            collection.CollectionChanged -= MainCollection_CollectionChanged;
    }

    internal class Group : IDisposable
    {
        private readonly int _headOrFooterCount;
        private readonly bool _usedFooter;
        private readonly WeakReference<SourceProvider> _sourceProvider;
        private readonly bool _usedHeader;
        private readonly INotifyCollectionChanged? _notifyCollectionChanged;

        public Group(SourceProvider sourceProvider, object context, int wideIndex, bool useHeader, bool useFooter)
        {
            _usedHeader = useHeader;
            _usedFooter = useFooter;
            _sourceProvider = new(sourceProvider);
            Context = context;
            WideIndex = wideIndex;

            if (context is INotifyCollectionChanged collection)
            {
                _notifyCollectionChanged = collection;
                _notifyCollectionChanged.CollectionChanged += Collection_CollectionChanged;
            }

            if (useHeader)
            {
                _headOrFooterCount++;
                CountElements++;
            }

            if (context is IList list)
            {
                CountElements += list.Count;
            }

            if (useFooter)
            {
                _headOrFooterCount++;
                CountElements++;
            }
        }

        public int WideIndex { get; set; }
        public object Context { get; private set; }

        public int CountElements { get; private set; }
        public object[] Items
        {
            get
            {
                var list = new List<object>();

                if (_usedHeader)
                    list.Add(Context);

                if (Context is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                        list.Add(item);
                    return list.ToArray();
                }
                else
                {
                    list.Add(Context);
                }

                if (_usedFooter)
                    list.Add(Context);

                return list.ToArray();
            }
        }

        public void Dispose()
        {
            if (_notifyCollectionChanged != null)
                _notifyCollectionChanged.CollectionChanged -= Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    CountElements++;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    CountElements--;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    CountElements = 0 + _headOrFooterCount;
                    break;
                default:
                    break;
            }

            if (_sourceProvider.TryGetTarget(out var source))
            {
                source.InnerGroup_CollectionChanged(sender, e);
            }
        }
    }

    [DebuggerDisplay("{Index} | {Context}")]
    private class SourceItem
    {
        public SourceItem(TemplateItemType type, object context)
        {
            Type = type;
            Context = context;
        }

        public TemplateItemType Type { get; }
        public object Context { get; }
    }
}