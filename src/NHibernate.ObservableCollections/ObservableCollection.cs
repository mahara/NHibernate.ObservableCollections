using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Iesi.Collections.Generic;

/// <summary>
///     A generic collection that fires events
///     when item(s) have been added to or removed from the collection.
/// </summary>
/// <typeparam name="T">
///     The type of items in the collection.
/// </typeparam>
/// <remarks>
///     REFERENCES:
///     -   <see href="https://github.com/dotnet/designs/pull/320" />
///     -   <see href="https://github.com/dotnet/runtime/issues/18087" />
///     -   <see href="https://github.com/dotnet/wpf/pull/9568" />
///     -   <see href="https://gist.github.com/weitzhandler/65ac9113e31d12e697cb58cd92601091" />
///         -   <see href="https://stackoverflow.com/questions/670577/observablecollection-doesnt-support-addrange-method-so-i-get-notified-for-each" />
///     -   <see href="https://github.com/CodingOctocat/WpfObservableRangeCollection" />
///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs" />
///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/ObjectModel/Collection.cs" />
///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs" />
///     -   <see href="https://happynomad121.blogspot.com/2007/12/collections-for-wpf-and-nhibernate.html" />
///     -   <see href="https://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs" />
///     -   <see href="https://referencesource.microsoft.com/#mscorlib/system/collections/objectmodel/collection.cs" />
///     -   <see href="https://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html" />
/// </remarks>
[Serializable]
[DebuggerTypeProxy(typeof(CollectionDebugView<>))]
[DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
public class ObservableCollection<T> :
    Collection<T>,
    INotifyCollectionChanged, INotifyPropertyChanged
{
    private SimpleMonitor? _monitor; // Lazily allocated only when a subclass calls BlockReentrancy() or during serialization. Do not rename (binary serialization).

    [NonSerialized]
    private int _blockReentrancyCount;

    [NonSerialized]
    private DeferredEventArgsCollectionExecution? _deferredEventArgsCollectionExecution;

    /// <summary>
    ///     Initializes a new instance of <see cref="ObservableCollection{T}" />
    ///     that is empty and has default initial capacity.
    /// </summary>
    public ObservableCollection()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableCollection{T}" /> class that contains elements
    ///     copied from the specified collection and has sufficient capacity
    ///     to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new collection.</param>
    /// <remarks>
    ///     The elements are copied onto the <see cref="ObservableCollection{T}" />
    ///     in the same order they are read by the enumerator of the collection.
    /// </remarks>
    /// <exception cref="ArgumentNullException"> collection is a null reference </exception>
    public ObservableCollection(IEnumerable<T> collection) :
        base(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection))))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableCollection{T}" /> class
    ///     that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="list">The list whose elements are copied to the new collection.</param>
    /// <remarks>
    ///     The elements are copied onto the <see cref="ObservableCollection{T}" />
    ///     in the same order they are read by the enumerator of the list.
    /// </remarks>
    /// <exception cref="ArgumentNullException"> list is a null reference </exception>
    public ObservableCollection(List<T> list) :
        base(new List<T>(list ?? throw new ArgumentNullException(nameof(list))))
    {
    }

    /// <summary>
    ///     Occurs when the collection changes, either by adding or removing an item.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public ReadOnlyCollection<NotifyCollectionChangedEventHandler> CollectionChangedEventHandlers
    {
        get
        {
            if (CollectionChanged is not NotifyCollectionChangedEventHandler handler)
            {
#if NET8_0_OR_GREATER
                return ReadOnlyCollection<NotifyCollectionChangedEventHandler>.Empty;
#else
                return ReadOnlyCollectionInternal<NotifyCollectionChangedEventHandler>.Empty;
#endif
            }

            var invocationList = handler.GetInvocationList();
            if (invocationList.Length == 0)
            {
#if NET8_0_OR_GREATER
                return ReadOnlyCollection<NotifyCollectionChangedEventHandler>.Empty;
#else
                return ReadOnlyCollectionInternal<NotifyCollectionChangedEventHandler>.Empty;
#endif
            }

            var handlers = invocationList.Cast<NotifyCollectionChangedEventHandler>()
                                         .ToArray();
            return new ReadOnlyCollection<NotifyCollectionChangedEventHandler>(handlers);
        }
    }

    /// <summary>
    ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
    /// </summary>
    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add => PropertyChanged += value;
        remove => PropertyChanged -= value;
    }

    protected event PropertyChangedEventHandler? PropertyChanged;

    public ReadOnlyCollection<PropertyChangedEventHandler> PropertyChangedEventHandlers
    {
        get
        {
            if (PropertyChanged is not PropertyChangedEventHandler handler)
            {
#if NET8_0_OR_GREATER
                return ReadOnlyCollection<PropertyChangedEventHandler>.Empty;
#else
                return ReadOnlyCollectionInternal<PropertyChangedEventHandler>.Empty;
#endif
            }

            var invocationList = handler.GetInvocationList();
            if (invocationList.Length == 0)
            {
#if NET8_0_OR_GREATER
                return ReadOnlyCollection<PropertyChangedEventHandler>.Empty;
#else
                return ReadOnlyCollectionInternal<PropertyChangedEventHandler>.Empty;
#endif
            }

            var handlers = invocationList.Cast<PropertyChangedEventHandler>()
                                         .ToArray();
            return new ReadOnlyCollection<PropertyChangedEventHandler>(handlers);
        }
    }

    protected bool EventNotificationsAreDeferred
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _deferredEventArgsCollectionExecution is not null;
    }

    /// <summary>
    ///     Raises a change notification indicating that all bindings should be refreshed.
    /// </summary>
    public void Refresh()
    {
        RefreshItems();
    }

    protected virtual void RefreshItems()
    {
        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionReset();
    }

    /// <summary>
    ///     Called by base class <see cref="Collection{T}" /> when an item is added to collection;
    ///     raises a <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected override void InsertItem(int index, T item)
    {
        CheckReentrancy();

        base.InsertItem(index, item);

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
    }

    /// <summary>
    ///     Called by base class <see cref="Collection{T}" /> when an item is removed from collection;
    ///     raises a <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected override void RemoveItem(int index)
    {
        CheckReentrancy();

        var itemRemoved = this[index];
        base.RemoveItem(index);

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, itemRemoved, index);
    }

    /// <summary>
    ///     Called by base class <see cref="Collection{T}" /> when an item is set in collection;
    ///     raises a <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected override void SetItem(int index, T item)
    {
        CheckReentrancy();

        var itemOld = this[index];
        base.SetItem(index, item);

        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Replace, itemOld, item, index);
    }

    /// <summary>
    ///     Moves an item at <paramref name="oldIndex" /> to <paramref name="newIndex" />.
    /// </summary>
    public void Move(int oldIndex, int newIndex)
    {
        MoveItem(oldIndex, newIndex);
    }

    /// <summary>
    ///     Called by base class <see cref="Collection{T}" /> when an item is to be moved within the collection;
    ///     raises a <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected virtual void MoveItem(int oldIndex, int newIndex)
    {
        CheckReentrancy();

        var itemMoved = this[oldIndex];
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, itemMoved);

        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Move, itemMoved, oldIndex, newIndex);
    }

    /// <summary>
    ///     Called by base class <see cref="Collection{T}" /> when the collection is being cleared;
    ///     raises a <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected override void ClearItems()
    {
        using var _ = BlockReentrancy();
        using var __ = DeferEventNotifications();

        CheckReentrancy();

        base.ClearItems();

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionReset();
    }

    /// <summary>
    ///     Adds a range to the end of the collection.
    ///     Raises CollectionChanged (NotifyCollectionChangedAction.Add).
    /// </summary>
    /// <param name="collection"></param>
    public void AddRange(IEnumerable<T> collection)
    {
        InsertItemsRange(Count, collection);
    }

    /// <summary>
    ///     Inserts a range.
    ///     Raises CollectionChanged (NotifyCollectionChangedAction.Add).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="collection"></param>
    public void InsertRange(int index, IEnumerable<T> collection)
    {
        InsertItemsRange(index, collection);
    }

    protected virtual void InsertItemsRange(int index, IEnumerable<T> collection)
    {
        using var _ = BlockReentrancy();
        using var __ = DeferEventNotifications();

        if (index < 0 || index > Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (!collection.Any())
        {
            return;
        }

        CheckReentrancy();

        var items = (List<T>) Items;

        var itemsAdded = collection.ToList();
        items.InsertRange(index, itemsAdded);

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Add, itemsAdded, index);
    }

    /// <summary>
    ///     Removes a range.
    ///     Raises CollectionChanged (NotifyCollectionChangedAction.Remove).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="count"></param>
    public void RemoveRange(int index, int count)
    {
        RemoveItemsRange(index, count);
    }

    protected virtual void RemoveItemsRange(int index, int count)
    {
        using var _ = BlockReentrancy();
        using var __ = DeferEventNotifications();

        if (index < 0 || index > Count || index + count > Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        CheckReentrancy();

        var items = (List<T>) Items;

        //var itemsRemoved = items.GetRange(index, count);
        var itemsRemoved = items.GetRange(index..(index + count));
        items.RemoveRange(index, count);

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        if (Count == 0)
        {
            OnCollectionReset();
        }
        else
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, itemsRemoved, index);
        }
    }

    /// <summary>
    ///     Removes a range.
    ///     Raises CollectionChanged (NotifyCollectionChangedAction.Remove).
    /// </summary>
    /// <param name="collection"></param>
    public void RemoveRange(IEnumerable<T> collection)
    {
        RemoveItemsRange(collection);
    }

    protected virtual void RemoveItemsRange(IEnumerable<T> collection)
    {
        using var _ = BlockReentrancy();
        using var __ = DeferEventNotifications();

#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(collection);
#else
        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }
#endif

        if (Count == 0)
        {
            return;
        }
        else if (collection is ICollection<T> countable)
        {
            if (countable.Count == 0)
            {
                return;
            }
            else if (countable.Count == 1)
            {
                using var enumerator = countable.GetEnumerator();

                enumerator.MoveNext();

                Remove(enumerator.Current);

                return;
            }
        }
        else if (!collection.Any())
        {
            return;
        }

        CheckReentrancy();

        var clusters = new Dictionary<int, List<T>>();
        var lastIndex = -1;
        List<T>? lastCluster = null;
        foreach (var item in collection)
        {
            var index = IndexOf(item);

            if (index < 0)
            {
                continue;
            }

            base.RemoveItem(index);

            if (lastIndex == index && lastCluster is not null)
            {
                lastCluster.Add(item);
            }
            else
            {
                clusters[lastIndex = index] = lastCluster = [item];
            }
        }

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        if (Count == 0)
        {
            OnCollectionReset();
        }
        else
        {
            foreach (var cluster in clusters)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, cluster.Value, cluster.Key);
            }
        }
    }

    /// <summary>
    ///     Replace a range with fewer, equal, or more items.
    ///     Raises CollectionChanged (NotifyCollectionChangedAction.Replace).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="count"></param>
    /// <param name="collection"></param>
    public void ReplaceRange(int index, int count, IEnumerable<T> collection)
    {
        ReplaceItemsRange(index, count, collection);
    }

    protected virtual void ReplaceItemsRange(int index, int count, IEnumerable<T> collection)
    {
        using var _ = BlockReentrancy();
        using var __ = DeferEventNotifications();

        if (index < 0 || index > Count || index + count > Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(collection);
#else
        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }
#endif

        var items_ItemsToReplace_IndexStart = index;
        var items_ItemsToReplace_Count = count;
        var itemsToReplace = collection;

        if (!itemsToReplace.Any())
        {
            return;
        }

        CheckReentrancy();

        var items = (List<T>) Items;

        var itemsOld = items.ToList();
        var itemsOldCount = itemsOld.Count;

        //var itemsRemoved = items.GetRange(items_ItemsToReplace_IndexStart, items_ItemsToReplace_Count);
        items.RemoveRange(items_ItemsToReplace_IndexStart, items_ItemsToReplace_Count);

        var itemsInserted = itemsToReplace.ToList();
        items.InsertRange(items_ItemsToReplace_IndexStart, itemsInserted);

        var itemsNew = items;
        var itemsNewCount = itemsNew.Count;

        if (itemsNewCount >= itemsOldCount)
        {
            //
            // ReplaceRange
            //
            OnIndexerPropertyChanged();
            OnCollectionChanged(
                NotifyCollectionChangedAction.Replace,
                //itemsOld.GetRange(items_ItemsToReplace_IndexStart, itemsOldCount - items_ItemsToReplace_IndexStart),
                itemsOld.GetRange(items_ItemsToReplace_IndexStart..itemsOldCount),
                //itemsNew.GetRange(items_ItemsToReplace_IndexStart, itemsOldCount - items_ItemsToReplace_IndexStart),
                itemsNew.GetRange(items_ItemsToReplace_IndexStart..itemsOldCount),
                items_ItemsToReplace_IndexStart);

            //
            // Replace
            //
            //OnIndexerPropertyChanged();
            //for (var i = items_ItemsToReplace_IndexStart; i < itemsOldCount; i++)
            //{
            //    var itemOld = itemsOld[i];
            //    var itemNew = itemsNew[i];

            //    OnIndexerPropertyChanged();
            //    OnCollectionChanged(NotifyCollectionChangedAction.Replace, itemOld, itemNew, i);
            //}

            //
            // AddRange
            //
            if (itemsNewCount > itemsOldCount)
            {
                //var itemsNewAdded = itemsNew.GetRange(itemsOldCount, itemsNewCount - itemsOldCount);
                var itemsNewAdded = itemsNew.GetRange(itemsOldCount..itemsNewCount);
                var itemsNewAddedCount = itemsNewAdded.Count;

                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Add, itemsNewAdded, itemsNewAddedCount);
            }
        }
        else
        {
            //
            // ReplaceRange
            //
            OnIndexerPropertyChanged();
            OnCollectionChanged(
                NotifyCollectionChangedAction.Replace,
                //itemsOld.GetRange(items_ItemsToReplace_IndexStart, itemsNewCount - items_ItemsToReplace_IndexStart),
                itemsOld.GetRange(items_ItemsToReplace_IndexStart..itemsNewCount),
                //itemsNew.GetRange(items_ItemsToReplace_IndexStart, itemsNewCount - items_ItemsToReplace_IndexStart),
                itemsNew.GetRange(items_ItemsToReplace_IndexStart..itemsNewCount),
                items_ItemsToReplace_IndexStart);

            //
            // Replace
            //
            //OnIndexerPropertyChanged();
            //for (var i = items_ItemsToReplace_IndexStart; i < itemsNewCount; i++)
            //{
            //    var itemOld = itemsOld[i];
            //    var itemNew = itemsNew[i];

            //    OnIndexerPropertyChanged();
            //    OnCollectionChanged(NotifyCollectionChangedAction.Replace, itemOld, itemNew, i);
            //}

            //var itemsOldRemoved = itemsOld.GetRange(itemsNewCount, itemsOldCount - itemsNewCount);
            var itemsOldRemoved = itemsOld.GetRange(itemsNewCount..itemsOldCount);
            var itemsNewRemovedCount = itemsOldRemoved.Count;

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, itemsOldRemoved, itemsNewCount);
        }
    }

    protected virtual IDisposable DeferEventNotifications()
    {
        return new DeferredEventArgsCollectionExecution(this);
    }

    /// <summary>
    ///     Helper to raise a ranged <see cref="CollectionChanged" /> event with action == Reset to any listeners.
    /// </summary>
    protected void OnCollectionReset()
    {
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }

    /// <summary>
    ///     Helper to raise a <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
    }

    /// <summary>
    ///     Helper to raise a <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int indexOld, int indexNew)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, indexNew, indexOld));
    }

    /// <summary>
    ///     Helper to raise a <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object? itemOld, object? itemNew, int index)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, itemNew, itemOld, index));
    }

    /// <summary>
    ///     Helper to raise a ranged <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, int indexStarting)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items, indexStarting));
    }

    /// <summary>
    ///     Helper to raise a ranged <see cref="CollectionChanged" /> event to any listeners.
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, IList itemsOld, IList itemsNew, int indexStarting)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, itemsNew, itemsOld, indexStarting));
    }

    /// <summary>
    ///     Raises a <see cref="CollectionChanged" /> event to any listeners.
    ///     Properties/methods modifying this <see cref="ObservableCollection{T}" /> instance will raise
    ///     a collection changed event through this virtual method.
    /// </summary>
    /// <remarks>
    ///     When overriding this method, either call its base implementation
    ///     or call <see cref="BlockReentrancy" /> to guard against reentrant collection changes.
    /// </remarks>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (_deferredEventArgsCollectionExecution is not null)
        {
            _deferredEventArgsCollectionExecution.AddEventArgs(e);

            return;
        }

        if (CollectionChanged is NotifyCollectionChangedEventHandler handler)
        {
            // Not calling BlockReentrancy() here to avoid the SimpleMonitor allocation.
            _blockReentrancyCount++;

            try
            {
                handler(this, e);
            }
            finally
            {
                _blockReentrancyCount--;
            }
        }
    }

    /// <summary>
    ///     Helper to raise a PropertyChanged event for the Count property.
    /// </summary>
    protected void OnCountPropertyChanged()
    {
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    /// <summary>
    ///     Helper to raise a PropertyChanged event for the Indexer property.
    /// </summary>
    protected void OnIndexerPropertyChanged()
    {
        OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
    }

    /// <summary>
    ///     Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
    /// </summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (_deferredEventArgsCollectionExecution is not null)
        {
            _deferredEventArgsCollectionExecution.AddEventArgs(e);

            return;
        }

        if (PropertyChanged is PropertyChangedEventHandler handler)
        {
            handler(this, e);
        }
    }

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
        EnsureMonitorInitialized();

        _monitor!._busyCount = _blockReentrancyCount;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (_monitor is not null)
        {
            _blockReentrancyCount = _monitor._busyCount;
            _monitor._collection = this;
        }
    }

    /// <summary>
    ///     Disallow reentrant attempts to change this collection.
    ///     E.g. an event handler of the <see cref="CollectionChanged" /> event
    ///     is not allowed to make changes to this collection.
    /// </summary>
    /// <remarks>
    ///     Typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
    ///     <code>
    ///         using (BlockReentrancy())
    ///         {
    ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
    ///         }
    ///     </code>
    /// </remarks>
    protected IDisposable BlockReentrancy()
    {
        _blockReentrancyCount++;

        return EnsureMonitorInitialized();
    }

    /// <summary>
    ///     Check and assert for reentrant attempts to change this collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Raised when changing the collection while another collection change is still being notified to other listeners.
    /// </exception>
    protected void CheckReentrancy()
    {
        if (_blockReentrancyCount > 0)
        {
            // We can allow changes if there's only one listener -
            // the problem only arises if reentrant changes make the original event args
            // invalid for later listeners.
            // This keeps existing code working (e.g. Selector.SelectedItems).
            if (CollectionChanged is NotifyCollectionChangedEventHandler handler &&
#if NET9_0_OR_GREATER
                !handler.HasSingleTarget
#else
                handler.GetInvocationList().Length > 1
#endif
               )
            {
                throw new InvalidOperationException(SR.ObservableCollectionReentrancyNotAllowed);
            }
        }
    }

    private SimpleMonitor EnsureMonitorInitialized()
    {
        return _monitor ??= new SimpleMonitor(this);
    }

    // This class helps prevent reentrant calls.
    [Serializable]
    private sealed class SimpleMonitor : IDisposable
    {
        internal int _busyCount; // Only used during (de)serialization to maintain compatibility with desktop. Do not rename (binary serialization).

        [NonSerialized]
        internal ObservableCollection<T> _collection;

        public SimpleMonitor(ObservableCollection<T> collection)
        {
            Debug.Assert(collection is not null);

            _collection = collection!;
        }

        public void Dispose()
        {
            _collection._blockReentrancyCount--;
        }
    }

    private sealed class DeferredEventArgsCollectionExecution : IDisposable
    {
        private readonly ObservableCollection<T> _collection;
        private readonly List<EventArgs> _eventArgsList = [];

        public DeferredEventArgsCollectionExecution(ObservableCollection<T> collection)
        {
            Debug.Assert(collection is not null);
            Debug.Assert(collection!._deferredEventArgsCollectionExecution is null);

            _collection = collection;
            _collection._deferredEventArgsCollectionExecution = this;
        }

        public void Dispose()
        {
            _collection._deferredEventArgsCollectionExecution = null;

            foreach (var e in _eventArgsList)
            {
                if (e is NotifyCollectionChangedEventArgs collectionChangedEventArgs)
                {
                    _collection.OnCollectionChanged(collectionChangedEventArgs);
                }
                else if (e is PropertyChangedEventArgs propertyChangedEventArgs)
                {
                    _collection.OnPropertyChanged(propertyChangedEventArgs);
                }
            }

            _eventArgsList.Clear();
        }

        public void AddEventArgs<TEventArgs>(TEventArgs e) where TEventArgs : EventArgs
        {
            _eventArgsList.Add(e);
        }
    }

    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new(nameof(Count));
        internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
    }

#if !NET8_0_OR_GREATER
    private class ReadOnlyCollectionInternal<TItem> : ReadOnlyCollection<TItem>
    {
        internal ReadOnlyCollectionInternal(IList<TItem> list) :
            base(list)
        {
        }

        /// <summary>
        ///     Gets an empty <see cref="ReadOnlyCollection{T}" />.
        /// </summary>
        /// <value>An empty <see cref="ReadOnlyCollection{T}" />.</value>
        /// <remarks>
        ///     The returned instance is immutable and will always be empty.
        /// </remarks>
        public static ReadOnlyCollection<TItem> Empty { get; } =
            new ReadOnlyCollection<TItem>(Array.Empty<TItem>());
    }
#endif
}
