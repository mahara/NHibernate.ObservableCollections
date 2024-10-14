using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using Iesi.Collections.Generic.Properties;

namespace Iesi.Collections.Generic
{
    /// <summary>
    ///     Represents a dynamic data collection that provides notifications
    ///     when items get added or removed, or when the whole collection is refreshed.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the collection.
    /// </typeparam>
    /// <remarks>
    ///     AUTHORS:
    ///     -   Microsoft Corporation
    ///     -   Maximilian Haru Raditya
    ///     -   Adrian Alexander
    ///     REFERENCES:
    ///     -   <see href="https://github.com/dotnet/designs/pull/320" />
    ///         -   <see href="https://github.com/dotnet/runtime/issues/18087" />
    ///         -   <see href="https://github.com/dotnet/wpf/pull/9568" />
    ///         -   <see href="https://github.com/dotnet/wpf/pull/10845" />
    ///     -   <see href="https://gist.github.com/weitzhandler/65ac9113e31d12e697cb58cd92601091" />
    ///         -   <see href="https://stackoverflow.com/questions/670577/observablecollection-doesnt-support-addrange-method-so-i-get-notified-for-each" />
    ///     -   <see href="https://github.com/CodingOctocat/WpfObservableRangeCollection" />
    ///     -   <see href="https://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html" />
    ///     -   <see href="https://happynomad121.blogspot.com/2007/12/collections-for-wpf-and-nhibernate.html" />
    ///     -   <see href="https://happynomad121.blogspot.com/2008/05/revisiting-bidirectional-assoc-helpers.html" />
    ///     -   <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/ObjectModel/Collection.cs" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs" />
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
        private DeferredEventNotificationsOperation? _deferredEventNotificationsOperation;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableCollection{T}" /> class.
        /// </summary>
        public ObservableCollection()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableCollection{T}" /> class
        ///     that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <remarks>
        ///     The elements are copied onto the <see cref="ObservableCollection{T}" />
        ///     in the same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="collection" /> parameter cannot be <see langword="null" />.</exception>
        public ObservableCollection(IEnumerable<T> collection) :
            base([.. collection ?? throw new ArgumentNullException(nameof(collection))])
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableCollection{T}" /> class
        ///     that contains elements copied from the specified list.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        /// <remarks>
        ///     The elements are copied onto the <see cref="ObservableCollection{T}" />
        ///     in the same order they are read by the enumerator of the list.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="list" /> parameter cannot be <see langword="null" />.</exception>
        public ObservableCollection(List<T> list) :
            base([.. list ?? throw new ArgumentNullException(nameof(list))])
        {
        }

        /// <summary>
        ///     Occurs when an item is added, removed, or moved, or the entire collection is refreshed.
        /// </summary>
        [field: NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

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

                var handlers = invocationList.Cast<NotifyCollectionChangedEventHandler>().ToList();
                return new ReadOnlyCollection<NotifyCollectionChangedEventHandler>(handlers);
            }
        }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public virtual event PropertyChangedEventHandler? PropertyChanged;

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

                var handlers = invocationList.Cast<PropertyChangedEventHandler>().ToList();
                return new ReadOnlyCollection<PropertyChangedEventHandler>(handlers);
            }
        }

        protected bool EventNotificationsAreDeferred
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _deferredEventNotificationsOperation is not null;
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" />'s <see cref="NotifyCollectionChangedAction.Reset" /> event to any listeners.
        /// </summary>
        public void Refresh()
        {
            RefreshItems();
        }

        protected virtual void RefreshItems()
        {
            OnCollectionReset();

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Called by base <see cref="Collection{T}" /> when an item is set in collection.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Replace" />) event to any listeners.
        /// </remarks>
        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();

            var itemOld = this[index];

            base.SetItem(index, item);

            OnCollectionChanged(itemOld, item, index);

            OnIndexerPropertyChanged();
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Called by base <see cref="Collection{T}" /> when an item is added to collection.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Add" />) event to any listeners.
        /// </remarks>
        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();

            base.InsertItem(index, item);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Called by base <see cref="Collection{T}" /> when an item is removed from collection.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) event to any listeners.
        /// </remarks>
        protected override void RemoveItem(int index)
        {
            if (Count == 0)
            {
                return;
            }

            CheckReentrancy();

            var itemRemoved = this[index];

            base.RemoveItem(index);

            OnCollectionChanged(NotifyCollectionChangedAction.Remove, itemRemoved, index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Move" />) event to any listeners.
        /// </remarks>
        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(oldIndex));
            }

            if (newIndex < 0 || newIndex >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            if (Count == 0)
            {
                return;
            }

            MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        ///     Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <remarks>
        ///     Called by <see cref="Collection{T}" /> when an item is to be moved within the collection.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Move" />) event to any listeners.
        /// </remarks>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            var itemMoved = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, itemMoved);

            OnCollectionChanged(itemMoved, oldIndex, newIndex);

            OnIndexerPropertyChanged();
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Called by base <see cref="Collection{T}" /> when the collection is being cleared.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Reset" />) event to any listeners.
        /// </remarks>
        protected override void ClearItems()
        {
            if (Count == 0)
            {
                return;
            }

            CheckReentrancy();

            using var _ = BlockReentrancy();

            base.ClearItems();

            OnCollectionReset();

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Adds a range of items to the end of the <see cref="ObservableCollection{T}" />.
        /// </summary>
        /// <param name="collection"></param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Add" />) events.
        /// </remarks>
        public void AddRange(IEnumerable<T> collection)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(collection);
#else
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
#endif

            InsertItemsRange(Count, collection);
        }

        /// <summary>
        ///     Inserts a range of items into the <see cref="ObservableCollection{T}" /> at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="collection"></param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Add" />) events.
        /// </remarks>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (index < 0 || index > Count)
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

            InsertItemsRange(index, collection);
        }

        /// <summary>
        ///     Inserts a range of items into the <see cref="ObservableCollection{T}" /> at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="collection"></param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Add" />) events.
        /// </remarks>
        protected virtual void InsertItemsRange(int index, IEnumerable<T> collection)
        {
            if (collection is ICollection<T> countable)
            {
                if (countable.Count == 0)
                {
                    return;
                }
            }
            else if (!collection.Any())
            {
                return;
            }

            CheckReentrancy();

            using var _ = BlockReentrancy();
            using var __ = DeferEventNotifications();

            var items = (List<T>) Items;

            var itemsAdded = collection.ToArray();

            items.InsertRange(index, itemsAdded);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, itemsAdded, index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Removes a range of items starting at the specified index of the <see cref="ObservableCollection{T}" />.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) events.
        /// </remarks>
        public void RemoveRange(int index, int count)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0 || count > Count - index)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                return;
            }

            //if (Count == 0)
            //{
            //    return;
            //}

            RemoveItemsRange(index, count);
        }

        /// <summary>
        ///     Removes a range of items from the <see cref="ObservableCollection{T}" /> starting at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) events.
        /// </remarks>
        protected virtual void RemoveItemsRange(int index, int count)
        {
            CheckReentrancy();

            using var _ = BlockReentrancy();
            using var __ = DeferEventNotifications();

            var items = (List<T>) Items;

            var itemsRemoved = items.GetRange(index, count);
            //var itemsRemoved = items.GetRange(index..(index + count));

            items.RemoveRange(index, count);

            if (Count == 0)
            {
                OnCollectionReset();
            }
            else
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, itemsRemoved, index);
            }

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Removes a range of items from the <see cref="ObservableCollection{T}" />.
        /// </summary>
        /// <param name="collection"></param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) events.
        /// </remarks>
        public void RemoveRange(IEnumerable<T> collection)
        {
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

            RemoveItemsRange(collection);
        }

        /// <summary>
        ///     Removes a range of items from the <see cref="ObservableCollection{T}" />.
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) events.
        /// </remarks>
        protected virtual void RemoveItemsRange(IEnumerable<T> collection)
        {
            if (collection is ICollection<T> countable)
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

            using var _ = BlockReentrancy();
            using var __ = DeferEventNotifications();

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

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Replaces a range of items within the <see cref="ObservableCollection{T}" /> with fewer, equal, or more items.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="collection"></param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Replace" />,
        ///     <see cref="NotifyCollectionChangedAction.Add" />, and/or <see cref="NotifyCollectionChangedAction.Remove" />) events.
        /// </remarks>
        public void ReplaceRange(int index, int count, IEnumerable<T> collection)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0 || count > Count - index)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(collection);
#else
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
#endif

            if (count == 0)
            {
                return;
            }

            //if (Count == 0)
            //{
            //    return;
            //}

            ReplaceItemsRange(index, count, collection);
        }

        /// <summary>
        ///     Replaces a range of items within the <see cref="ObservableCollection{T}" /> with fewer, equal, or more items.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="collection"></param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Replace" />,
        ///     <see cref="NotifyCollectionChangedAction.Add" />, and/or <see cref="NotifyCollectionChangedAction.Remove" />) events.
        /// </remarks>
        protected virtual void ReplaceItemsRange(int index, int count, IEnumerable<T> collection)
        {
            var items_ItemsToReplace_IndexStart = index;
            var items_ItemsToReplace_Count = count;
            var itemsToReplace = collection;

            if (!itemsToReplace.Any())
            {
                return;
            }

            CheckReentrancy();

            using var _ = BlockReentrancy();
            using var __ = DeferEventNotifications();

            var items = (List<T>) Items;

            var itemsOld = (List<T>) [.. items];
            var itemsOldCount = itemsOld.Count;

            //
            // RemoveRange
            //
            //var itemsRemoved = items.GetRange(items_ItemsToReplace_IndexStart, items_ItemsToReplace_Count);

            items.RemoveRange(items_ItemsToReplace_IndexStart, items_ItemsToReplace_Count);

            //
            // InsertRange
            //
            var itemsInserted = (List<T>) [.. itemsToReplace];

            items.InsertRange(items_ItemsToReplace_IndexStart, itemsInserted);

            var itemsNew = items;
            var itemsNewCount = itemsNew.Count;

            if (itemsNewCount >= itemsOldCount)
            {
                //
                // ReplaceRange Events
                //
                var itemsOldRemoved_ReplaceRange = itemsOld.GetRange(items_ItemsToReplace_IndexStart, itemsOldCount - items_ItemsToReplace_IndexStart);
                //var itemsOldRemoved_ReplaceRange = itemsOld.GetRange(items_ItemsToReplace_IndexStart..itemsOldCount);

                var itemsNewAdded_ReplaceRange = itemsNew.GetRange(items_ItemsToReplace_IndexStart, itemsOldCount - items_ItemsToReplace_IndexStart);
                //var itemsNewAdded_ReplaceRange = itemsNew.GetRange(items_ItemsToReplace_IndexStart..itemsOldCount);

                OnCollectionChanged(
                    NotifyCollectionChangedAction.Replace,
                    itemsOldRemoved_ReplaceRange,
                    itemsNewAdded_ReplaceRange,
                    items_ItemsToReplace_IndexStart);



                //
                // Replace Events
                //
                //for (var i = items_ItemsToReplace_IndexStart; i < itemsOldCount; i++)
                //{
                //    var itemOld = itemsOld[i];
                //    var itemNew = itemsNew[i];

                //    OnCollectionChanged(
                //        NotifyCollectionChangedAction.Replace,
                //        itemOld,
                //        itemNew,
                //        i);
                //}



                //
                // AddRange Events
                //
                if (itemsNewCount > itemsOldCount)
                {
                    var itemsNewAdded_AddRange = itemsNew.GetRange(itemsOldCount, itemsNewCount - itemsOldCount);
                    //var itemsNewAdded = itemsNew.GetRange(itemsOldCount..itemsNewCount);
                    var itemsNewAddedCount_AddRange = itemsNewAdded_AddRange.Count;

                    OnCountPropertyChanged();
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(
                        NotifyCollectionChangedAction.Add,
                        itemsNewAdded_AddRange,
                        itemsNewAddedCount_AddRange);
                }
            }
            else
            {
                //
                // ReplaceRange Events
                //
                var itemsOldRemoved_ReplaceRange = itemsOld.GetRange(items_ItemsToReplace_IndexStart, itemsNewCount - items_ItemsToReplace_IndexStart);
                //var itemsOldRemoved_ReplaceRange = itemsOld.GetRange(items_ItemsToReplace_IndexStart..itemsNewCount);

                var itemsNewAdded_ReplaceRange = itemsNew.GetRange(items_ItemsToReplace_IndexStart, itemsNewCount - items_ItemsToReplace_IndexStart);
                //var itemsNewAdded_ReplaceRange = itemsNew.GetRange(items_ItemsToReplace_IndexStart..itemsNewCount);

                OnCollectionChanged(
                    NotifyCollectionChangedAction.Replace,
                    itemsOldRemoved_ReplaceRange,
                    itemsNewAdded_ReplaceRange,
                    items_ItemsToReplace_IndexStart);



                //
                // Replace Events
                //
                //for (var i = items_ItemsToReplace_IndexStart; i < itemsNewCount; i++)
                //{
                //    var itemOld = itemsOld[i];
                //    var itemNew = itemsNew[i];

                //    OnCollectionChanged(
                //        NotifyCollectionChangedAction.Replace,
                //        itemOld,
                //        itemNew,
                //        i);
                //}



                //
                // RemoveRange Events
                //
                var itemsOldRemoved_RemoveRange = itemsOld.GetRange(itemsNewCount, itemsOldCount - itemsNewCount);
                //var itemsOldRemoved_RemoveRange = itemsOld.GetRange(itemsNewCount..itemsOldCount);

                OnCollectionChanged(
                    NotifyCollectionChangedAction.Remove,
                    itemsOldRemoved_RemoveRange,
                    itemsNewCount);

                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
            }
        }

        /// <summary>
        ///     Checks if there is currently no reentrancy that is making any changes to this collection.
        ///
        ///     -- ORIGINAL SUMMARY --
        ///     Check and assert for reentrant attempts to change this collection.
        /// </summary>
        /// <remarks>
        ///     This should be done before making any changes to this collection.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     Raised when changing the collection while another collection change is still being notified to other listeners.
        /// </exception>
        protected void CheckReentrancy()
        {
            if (_blockReentrancyCount > 0)
            {
                // We can allow changes if there's only one listener.
                // The problem only arises if reentrant changes make the original event args invalid for later listeners.
                // This keeps existing code working (e.g. Selector.SelectedItems).
                if (CollectionChanged is NotifyCollectionChangedEventHandler handler &&
                    handler.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException(Resources.ObservableCollection_ReentrancyNotAllowed);
                }
            }
        }

        /// <summary>
        ///     Blocks new reentrancy to prevent any changes to this collection.
        ///
        ///     -- ORIGINAL SUMMARY --
        ///     Disallow reentrant attempts to change this collection.
        ///     E.g. an event handler of the <see cref="CollectionChanged" /> event is not allowed to make changes to this collection.
        /// </summary>
        /// <remarks>
        ///     Typical usage is to wrap e.g. a <see cref="OnCollectionChanged(NotifyCollectionChangedEventArgs)" /> call with a using() scope:
        ///     <code>
        ///         using (BlockReentrancy())
        ///         {
        ///             OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
        ///         }
        ///     </code>
        /// </remarks>
        /// <returns>
        ///     A <see cref="SimpleMonitor" /> that blocks new reentrancy.
        /// </returns>
        protected IDisposable BlockReentrancy()
        {
            _blockReentrancyCount++;

            return EnsureMonitorInitialized();
        }

        private SimpleMonitor EnsureMonitorInitialized()
        {
            return _monitor ??= new SimpleMonitor(this);
        }

        protected virtual IDisposable DeferEventNotifications()
        {
            return new DeferredEventNotificationsOperation(this);
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Reset" />) event to any listeners.
        /// </summary>
        protected void OnCollectionReset()
        {
            OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Replace" />) event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(object? oldItem, object? newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Move" />) event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(object? item, int oldIndex, int newIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items, startingIndex));
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList oldItems, IList newItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItems, oldItems, startingIndex));
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        ///     Properties/methods modifying this <see cref="ObservableCollection{T}" /> will raise
        ///     a <see cref="CollectionChanged" /> event through this virtual method.
        /// </summary>
        /// <remarks>
        ///     When overriding this method, either call its base implementation
        ///     or call <see cref="BlockReentrancy" /> to guard against reentrant collection changes.
        /// </remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_deferredEventNotificationsOperation is not null)
            {
                _deferredEventNotificationsOperation.AddEventArgs(e);

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
        ///     Raises the <see cref="PropertyChanged" /> event for the Count property.
        /// </summary>
        protected void OnCountPropertyChanged()
        {
            OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event for the Indexer property.
        /// </summary>
        protected void OnIndexerPropertyChanged()
        {
            OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_deferredEventNotificationsOperation is not null)
            {
                _deferredEventNotificationsOperation.AddEventArgs(e);

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

        private sealed class DeferredEventNotificationsOperation : IDisposable
        {
            private readonly ObservableCollection<T> _collection;
            private readonly List<EventArgs> _eventArgsList = [];

            public DeferredEventNotificationsOperation(ObservableCollection<T> collection)
            {
                Debug.Assert(collection is not null);
                Debug.Assert(collection!._deferredEventNotificationsOperation is null);

                _collection = collection;
                _collection._deferredEventNotificationsOperation = this;
            }

            public void Dispose()
            {
                _collection._deferredEventNotificationsOperation = null;

                foreach (var eventArgs in _eventArgsList)
                {
                    if (eventArgs is NotifyCollectionChangedEventArgs collectionChangedEventArgs)
                    {
                        _collection.OnCollectionChanged(collectionChangedEventArgs);
                    }
                    else if (eventArgs is PropertyChangedEventArgs propertyChangedEventArgs)
                    {
                        _collection.OnPropertyChanged(propertyChangedEventArgs);
                    }
                }

                _eventArgsList.Clear();
            }

            public void AddEventArgs<TEventArgs>(TEventArgs eventArgs) where TEventArgs : EventArgs
            {
                _eventArgsList.Add(eventArgs);
            }
        }

        internal static class EventArgsCache
        {
            public static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
            public static readonly PropertyChangedEventArgs CountPropertyChanged = new(nameof(Count));
            public static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
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
            /// <remarks>The returned instance is immutable and will always be empty.</remarks>
            public static ReadOnlyCollection<TItem> Empty { get; } =
                new ReadOnlyCollection<TItem>(Array.Empty<TItem>());
        }
#endif
    }
}
