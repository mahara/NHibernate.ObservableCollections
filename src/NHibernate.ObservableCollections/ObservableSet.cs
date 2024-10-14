using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using Iesi.Collections.Generic.Properties;

namespace Iesi.Collections.Generic
{
    /// <summary>
    ///     Represents a dynamic data ordered set that provides notifications
    ///     when items get added or removed, or when the whole set is refreshed.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the set.
    /// </typeparam>
    /// <remarks>
    ///     AUTHORS:
    ///     -   Microsoft Corporation
    ///     -   Maximilian Haru Raditya
    ///     -   Adrian Alexander
    ///     REFERENCES:
    ///     -   <see href="https://happynomad121.blogspot.com/2007/12/collections-for-wpf-and-nhibernate.html" />
    ///     -   <see href="https://happynomad121.blogspot.com/2008/05/revisiting-bidirectional-assoc-helpers.html" />
    ///     -   <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.changetracking.observablehashset-1" />
    ///     -   <see href="https://github.com/dotnet/efcore/blob/main/src/EFCore/ChangeTracking/ObservableHashSet.cs" />
    ///     -   <see href="https://github.com/dotnet/efcore/blob/60524c9b11cdadb0d4be96adbe8d0954f9c7ed0a/src/EFCore/ChangeTracking/ObservableHashSet.cs" />
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
    public class ObservableSet<T> :
        ISet<T>, IReadOnlyList<T>, IReadOnlyCollection<T>,
        INotifyCollectionChanged, INotifyPropertyChanging, INotifyPropertyChanged
    {
        //
        //  NOTES:  Invariant conditions to preserve:
        //          -   _set owns uniqueness and membership using _set.Comparer.
        //          -   _list owns public order, enumeration order, indexer order, CopyTo order,
        //              and CollectionChanged indexes.
        //          -   _set.Count must always equal _list.Count.
        //          -   All items in _list must be in _set.
        //          -   No two items in _list may be equal according to _set.Comparer.
        //          -   Never rebuild _list from _set enumeration when preserving ordered-set semantics.
        //
        private readonly HashSet<T> _set;
        private readonly List<T> _list;

        private SimpleMonitor? _monitor; // Lazily allocated only when a subclass calls BlockReentrancy() or during serialization. Do not rename (binary serialization).

        [NonSerialized]
        private int _blockReentrancyCount;

        [NonSerialized]
        private DeferredEventNotificationsOperation? _deferredEventNotificationsOperation;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableSet{T}" /> class
        ///     that is empty and uses the default equality comparer for the set type.
        /// </summary>
        public ObservableSet() :
            this(EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableSet{T}" /> class
        ///     that uses the default equality comparer for the set type,
        ///     contains elements copied from the specified collection,
        ///     and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        public ObservableSet(IEnumerable<T> collection) :
            this(collection, EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableSet{T}" /> class
        ///     that is empty and uses the specified equality comparer for the set type.
        /// </summary>
        /// <param name="comparer">
        ///     The <see cref="IEqualityComparer{T}" /> implementation to use when comparing values in the set,
        ///     or null to use the default <see cref="IEqualityComparer{T}" /> implementation for the set type.
        /// </param>
        public ObservableSet(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
            _list = [];
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableSet{T}" /> class
        ///     that uses the specified equality comparer for the set type,
        ///     contains elements copied from the specified collection,
        ///     and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <param name="comparer">
        ///     The <see cref="IEqualityComparer{T}" /> implementation to use when comparing values in the set,
        ///     or null to use the default <see cref="IEqualityComparer{T}" /> implementation for the set type.
        /// </param>
        public ObservableSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
            _list = [];

            foreach (var item in collection)
            {
                if (_set.Add(item))
                {
                    _list.Add(item);
                }
            }
        }

        /// <summary>
        ///     Occurs when the contents of the <see cref="ObservableSet{T}" /> changes.
        /// </summary>
        [field: NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        ///     Occurs when a property of this <see cref="ObservableSet{T}" /> (such as <see cref="Count" />) is changing.
        /// </summary>
        [field: NonSerialized]
        public virtual event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        ///     Occurs when a property of this <see cref="ObservableSet{T}" /> (such as <see cref="Count" />) changes.
        /// </summary>
        [field: NonSerialized]
        public virtual event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Gets the number of elements that are contained in the <see cref="ObservableSet{T}" />.
        /// </summary>
        public int Count => _set.Count;

        /// <summary>
        ///     Gets a value indicating whether the <see cref="ObservableSet{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => ((ICollection<T>) _set).IsReadOnly;

        /// <summary>
        ///     Gets the <see cref="IEqualityComparer{T}" /> object that is used to determine equality for the values in the set.
        /// </summary>
        public IEqualityComparer<T> Comparer => _set.Comparer;

        /// <summary>
        ///     Gets the element at the specified index in the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the <see cref="ObservableSet{T}" />.</returns>
        public T this[int index] => _list[index];

        protected bool EventNotificationsAreDeferred
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _deferredEventNotificationsOperation is not null;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <returns>
        ///     An enumerator for the <see cref="ObservableSet{T}" />.
        /// </returns>
        /// <remarks>
        ///     WPF collection views observe this collection through <see cref="IEnumerable{T}" />
        ///     and validate <see cref="NotifyCollectionChangedEventArgs" /> indexes
        ///     against the visible enumeration order.
        ///
        ///     Therefore, enumeration order, indexer order, and CollectionChanged indexes
        ///     must all describe the same order. Do not enumerate <c>_set</c> if Add/Remove indexes
        ///     are calculated from <c>_list</c>.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" />'s <see cref="NotifyCollectionChangedAction.Reset" /> event to any listeners.
        /// </summary>
        public void Refresh()
        {
            VerifyState();

            RefreshItems();
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" />'s <see cref="NotifyCollectionChangedAction.Reset" /> event to any listeners.
        /// </summary>
        protected virtual void RefreshItems()
        {
            OnCountPropertyChanging();

            OnCollectionReset();

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> object contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in the <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> contains the specified element;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public bool Contains(T item)
        {
            VerifyState();

            return _set.Contains(item);
        }

        /// <summary>
        ///     Searches for the specified object and returns the zero-based index
        ///     of the first occurrence within the entire <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="item">
        ///     The object to locate in the <see cref="ObservableSet{T}" />. The value can be null for reference types.
        /// </param>
        /// <returns>
        ///     The zero-based index of the first occurrence of item within the entire <see cref="ObservableSet{T}" />, if found; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            VerifyState();

            var comparer = _set.Comparer;

            for (var i = 0; i < _list.Count; i++)
            {
                if (comparer.Equals(_list[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Adds the specified element to the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is added to the <see cref="ObservableSet{T}" />;
        ///     <see langword="false" /> if the element is already present.
        /// </returns>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Add" />) event to any listeners.
        /// </remarks>
        public bool Add(T item)
        {
            return Insert(Count, item);
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Add" />) event to any listeners.
        /// </remarks>
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        ///     Inserts the specified element into the <see cref="ObservableSet{T}" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The element to insert.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is inserted into the <see cref="ObservableSet{T}" />;
        ///     <see langword="false" /> if the element is already present.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than zero or greater than <see cref="Count" />.
        /// </exception>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Add" />) event to any listeners.
        /// </remarks>
        public bool Insert(int index, T item)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            VerifyState();

            if (index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (_set.Contains(item))
            {
                return false;
            }

            InsertItem(index, item);

            return true;
        }

        /// <summary>
        ///     Inserts the specified element into the <see cref="ObservableSet{T}" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The element to insert.</param>
        /// <remarks>
        ///     The caller must ensure that <paramref name="index" /> is valid
        ///     and that <paramref name="item" /> is not already present.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Add" />) event to any listeners.
        /// </remarks>
        protected virtual void InsertItem(int index, T item)
        {
            CheckReentrancy();

            OnCountPropertyChanging();

            var isAdded = _set.Add(item);

            Debug.Assert(isAdded);

            _list.Insert(index, item);

            VerifyState();

            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Removes the specified element from the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and removed;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) event to any listeners.
        /// </remarks>
        public bool Remove(T item)
        {
            var index = IndexOf(item);

            if (index < 0)
            {
                return false;
            }

            RemoveItem(index);

            return true;
        }

        /// <summary>
        ///     Removes the element at the specified index of the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) event to any listeners.
        /// </remarks>
        public void RemoveAt(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            VerifyState();

            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            RemoveItem(index);
        }

        /// <summary>
        ///     Removes the element at the specified index of the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <remarks>
        ///     The caller must ensure that <paramref name="index" /> is valid.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) event to any listeners.
        /// </remarks>
        protected virtual void RemoveItem(int index)
        {
            CheckReentrancy();

            var itemRemoved = _list[index];

            OnCountPropertyChanging();

            var isRemoved = _set.Remove(itemRemoved);

            Debug.Assert(isRemoved);

            _list.RemoveAt(index);

            VerifyState();

            OnCollectionChanged(NotifyCollectionChangedAction.Remove, itemRemoved, index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Moves the item at the specified index to a new location in the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Move" />) event to any listeners.
        /// </remarks>
        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(oldIndex));
            }

            if (newIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            VerifyState();

            if (oldIndex >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(oldIndex));
            }

            if (newIndex >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        ///     Moves the item at the specified index to a new location in the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        /// <remarks>
        ///     The caller must ensure that <paramref name="oldIndex" /> and <paramref name="newIndex" /> are valid.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Move" />) event to any listeners.
        /// </remarks>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            var itemMoved = _list[oldIndex];

            _list.RemoveAt(oldIndex);
            _list.Insert(newIndex, itemMoved);

            VerifyState();

            OnCollectionChanged(itemMoved, oldIndex, newIndex);

            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Removes all elements from the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Reset" />) event to any listeners.
        /// </remarks>
        public void Clear()
        {
            VerifyState();

            if (Count == 0)
            {
                return;
            }

            ClearItems();
        }

        /// <summary>
        ///     Removes all elements from the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Reset" />) event to any listeners.
        /// </remarks>
        protected virtual void ClearItems()
        {
            CheckReentrancy();

            using var _ = BlockReentrancy();

            OnCountPropertyChanging();

            _set.Clear();
            _list.Clear();

            VerifyState();

            OnCollectionReset();

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
        }

        /// <summary>
        ///     Sets the capacity of the <see cref="ObservableSet{T}" /> to the actual number of elements it contains,
        ///     rounded up to a nearby, implementation-specific value.
        /// </summary>
        public void TrimExcess()
        {
            VerifyState();

            TrimExcessItems();
        }

        /// <summary>
        ///     Sets the capacity of the <see cref="ObservableSet{T}" /> to the actual number of elements it contains,
        ///     rounded up to a nearby, implementation-specific value.
        /// </summary>
        protected virtual void TrimExcessItems()
        {
            _set.TrimExcess();
            _list.TrimExcess();

            VerifyState();
        }

        /// <summary>
        ///     Removes all elements that match the conditions defined by the specified predicate from the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="match">
        ///     The <see cref="Predicate{T}" /> delegate that defines the conditions of the elements to remove.
        /// </param>
        /// <returns>
        ///     The number of elements that were removed from the <see cref="ObservableSet{T}" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="match" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) events,
        ///     or a <see cref="NotifyCollectionChangedAction.Reset" /> event when all items are removed.
        /// </remarks>
        public int RemoveWhere(Predicate<T> match)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(match);
#else
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }
#endif

            VerifyState();

            if (Count == 0)
            {
                return 0;
            }

            return RemoveItemsWhere(match);
        }

        /// <summary>
        ///     Removes all elements that match the conditions defined by the specified predicate from the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="match">
        ///     The <see cref="Predicate{T}" /> delegate that defines the conditions of the elements to remove.
        /// </param>
        /// <returns>
        ///     The number of elements that were removed from the <see cref="ObservableSet{T}" />.
        /// </returns>
        /// <remarks>
        ///     The caller must ensure that <paramref name="match" /> is not <see langword="null" />.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) events,
        ///     or a <see cref="NotifyCollectionChangedAction.Reset" /> event when all items are removed.
        /// </remarks>
        protected virtual int RemoveItemsWhere(Predicate<T> match)
        {
            CheckReentrancy();

            using (BlockReentrancy())
            {
                using var deferredEventNotifications = DeferEventNotifications();

                var itemsRemovedCount = RemoveItemsWhereCore(match);

                deferredEventNotifications.Complete();

                return itemsRemovedCount;
            }
        }

        /// <summary>
        ///     Removes all elements that match the conditions defined by the specified predicate from the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="match">
        ///     The <see cref="Predicate{T}" /> delegate that defines the conditions of the elements to remove.
        /// </param>
        /// <returns>
        ///     The number of elements that were removed from the <see cref="ObservableSet{T}" />.
        /// </returns>
        /// <remarks>
        ///     The caller must ensure that <paramref name="match" /> is not <see langword="null" />.
        ///
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Remove" />) events,
        ///     or a <see cref="NotifyCollectionChangedAction.Reset" /> event when all items are removed.
        /// </remarks>
        protected virtual int RemoveItemsWhereCore(Predicate<T> match)
        {
            //
            // Phase 1: Create Removal Plan
            // - Read-only scan.
            // - Evaluate predicate and build removal clusters.
            //
            var itemRemovedClusters_Plan = new List<(int Index, List<T> Items)>();

            List<T>? itemRemovedCluster_Plan = null;
            var itemRemovedClusterIndex_Plan = -1;
            var itemsRemovedCount_Plan = 0;

            for (var indexOriginal = 0; indexOriginal < _list.Count; indexOriginal++)
            {
                var item = _list[indexOriginal];

                if (!match(item))
                {
                    itemRemovedCluster_Plan = null;
                    itemRemovedClusterIndex_Plan = -1;

                    continue;
                }

                var itemRemovedIndex = indexOriginal - itemsRemovedCount_Plan;

                if (itemRemovedCluster_Plan is not null &&
                    itemRemovedClusterIndex_Plan == itemRemovedIndex)
                {
                    itemRemovedCluster_Plan.Add(item);
                }
                else
                {
                    itemRemovedCluster_Plan = [item];
                    itemRemovedClusterIndex_Plan = itemRemovedIndex;

                    itemRemovedClusters_Plan.Add((itemRemovedIndex, itemRemovedCluster_Plan));
                }

                itemsRemovedCount_Plan++;
            }

            if (itemsRemovedCount_Plan == 0)
            {
                return 0;
            }

            //
            // Phase 2: Execute Removal Plan
            // - Commit.
            // - From here onward, mutation is allowed.
            //
            OnCountPropertyChanging();

            var itemsCount = Count;

            if (itemsRemovedCount_Plan == itemsCount)
            {
                _set.Clear();
                _list.Clear();

                OnCollectionReset();
            }
            else
            {
                foreach (var itemRemovedCluster in itemRemovedClusters_Plan)
                {
                    foreach (var itemRemoved in itemRemovedCluster.Items)
                    {
                        var isRemoved = _set.Remove(itemRemoved);

                        Debug.Assert(isRemoved);
                    }

                    _list.RemoveRange(
                        itemRemovedCluster.Index,
                        itemRemovedCluster.Items.Count);

                    OnCollectionChanged(
                        NotifyCollectionChangedAction.Remove,
                        itemRemovedCluster.Items,
                        itemRemovedCluster.Index);
                }
            }

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();

            VerifyState();

            return itemsRemovedCount_Plan;
        }

        /// <summary>
        ///     Modifies the <see cref="ObservableSet{T}" /> to contain all elements that are present in itself, the specified collection, or both.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        public virtual void UnionWith(IEnumerable<T> other)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(other);
#else
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }
#endif

            VerifyState();

            CheckReentrancy();

            using (BlockReentrancy())
            {
                using var deferredEventNotifications = DeferEventNotifications();

                //
                // Phase 1: Create Union Plan
                // - Read-only scan.
                // - Existing order is preserved.
                // - New unique items are appended in 'other' enumeration order.
                //
                var set_Plan = new HashSet<T>(_set, _set.Comparer);
                var list_ItemsAdded_Plan = new List<T>();

                foreach (var item in other)
                {
                    if (set_Plan.Add(item))
                    {
                        list_ItemsAdded_Plan.Add(item);
                    }
                }

                if (list_ItemsAdded_Plan.Count == 0)
                {
                    return;
                }

                //
                // Phase 2: Execute Union Plan
                // - Commit.
                //
                OnCountPropertyChanging();

                var index = Count;

                foreach (var itemAdded in list_ItemsAdded_Plan)
                {
                    var isAdded = _set.Add(itemAdded);

                    Debug.Assert(isAdded);
                }

                _list.AddRange(list_ItemsAdded_Plan);

                OnCollectionChanged(
                    NotifyCollectionChangedAction.Add,
                    list_ItemsAdded_Plan,
                    index);

                OnCountPropertyChanged();
                OnIndexerPropertyChanged();

                VerifyState();

                deferredEventNotifications.Complete();
            }
        }

        /// <summary>
        ///     Modifies the current <see cref="ObservableSet{T}" /> to contain only elements
        ///     that are present in that object and in the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        public virtual void IntersectWith(IEnumerable<T> other)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(other);
#else
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }
#endif

            VerifyState();

            if (Count == 0)
            {
                return;
            }

            CheckReentrancy();

            using (BlockReentrancy())
            {
                using var deferredEventNotifications = DeferEventNotifications();

                //
                // Phase 1: Create Intersection Plan
                // - Materialize 'other' before mutation.
                //
                var set_ItemsKept_Plan = new HashSet<T>(other, _set.Comparer);

                //
                // Phase 2: Execute Intersection Plan
                // - Remove every current item not present in 'other'.
                //
                RemoveItemsWhereCore(item => !set_ItemsKept_Plan.Contains(item));

                deferredEventNotifications.Complete();
            }
        }

        /// <summary>
        ///     Removes all elements in the specified collection from the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="other">The collection of items to remove from the current <see cref="ObservableSet{T}" />.</param>
        public virtual void ExceptWith(IEnumerable<T> other)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(other);
#else
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }
#endif

            VerifyState();

            if (Count == 0)
            {
                return;
            }

            CheckReentrancy();

            using (BlockReentrancy())
            {
                using var deferredEventNotifications = DeferEventNotifications();

                //
                // Phase 1: Create Removal Predicate (Except) Plan
                // - Materialize 'other' before mutation.
                //
                var set_ItemsRemoved_Plan = new HashSet<T>(other, _set.Comparer);

                if (set_ItemsRemoved_Plan.Count == 0)
                {
                    return;
                }

                //
                // Phase 2: Execute Removal Predicate (Except) Plan
                // - Remove matching current items while preserving survivor order.
                //
                RemoveItemsWhereCore(set_ItemsRemoved_Plan.Contains);

                deferredEventNotifications.Complete();
            }
        }

        /// <summary>
        ///     Modifies the current <see cref="ObservableSet{T}" /> to contain only elements that are present either in that object
        ///     or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        public virtual void SymmetricExceptWith(IEnumerable<T> other)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(other);
#else
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }
#endif

            VerifyState();

            CheckReentrancy();

            using (BlockReentrancy())
            {
                using var deferredEventNotifications = DeferEventNotifications();

                var comparer = _set.Comparer;

                //
                // Phase 1: Create Symmetric Difference (SymmetricExcept) Plan
                // - Read-only scan.
                //
                var set_ItemsSnapshot = new HashSet<T>(_set, comparer);
                var set_ItemsProcessed_Plan = new HashSet<T>(comparer);
                var set_ItemsRemoved_Plan = new HashSet<T>(comparer);
                var list_ItemsAdded_Plan = new List<T>();

                foreach (var item in other)
                {
                    if (!set_ItemsProcessed_Plan.Add(item))
                    {
                        continue;
                    }

                    if (set_ItemsSnapshot.Contains(item))
                    {
                        set_ItemsRemoved_Plan.Add(item);
                    }
                    else
                    {
                        list_ItemsAdded_Plan.Add(item);
                    }
                }

                if (set_ItemsRemoved_Plan.Count == 0 && list_ItemsAdded_Plan.Count == 0)
                {
                    return;
                }

                var itemRemovedClusters_Plan = new List<(int Index, List<T> Items)>();

                List<T>? itemRemovedCluster_Plan = null;
                var itemRemovedClusterIndex_Plan = -1;
                var itemsRemovedCount_Plan = 0;

                for (var indexOriginal = 0; indexOriginal < _list.Count; indexOriginal++)
                {
                    var item = _list[indexOriginal];

                    if (!set_ItemsRemoved_Plan.Contains(item))
                    {
                        itemRemovedCluster_Plan = null;
                        itemRemovedClusterIndex_Plan = -1;

                        continue;
                    }

                    var itemRemovedIndex = indexOriginal - itemsRemovedCount_Plan;

                    if (itemRemovedCluster_Plan is not null &&
                        itemRemovedClusterIndex_Plan == itemRemovedIndex)
                    {
                        itemRemovedCluster_Plan.Add(item);
                    }
                    else
                    {
                        itemRemovedCluster_Plan = [item];
                        itemRemovedClusterIndex_Plan = itemRemovedIndex;

                        itemRemovedClusters_Plan.Add((itemRemovedIndex, itemRemovedCluster_Plan));
                    }

                    itemsRemovedCount_Plan++;
                }

                //
                // Phase 2: Execute Symmetric Difference (SymmetricExcept) Plan
                // - Commit.
                //
                var countChanged = itemsRemovedCount_Plan != list_ItemsAdded_Plan.Count;

                if (countChanged)
                {
                    OnCountPropertyChanging();
                }

                foreach (var itemRemovedCluster in itemRemovedClusters_Plan)
                {
                    foreach (var itemRemoved in itemRemovedCluster.Items)
                    {
                        var isRemoved = _set.Remove(itemRemoved);

                        Debug.Assert(isRemoved);
                    }

                    _list.RemoveRange(
                        itemRemovedCluster.Index,
                        itemRemovedCluster.Items.Count);

                    OnCollectionChanged(
                        NotifyCollectionChangedAction.Remove,
                        itemRemovedCluster.Items,
                        itemRemovedCluster.Index);
                }

                if (list_ItemsAdded_Plan.Count > 0)
                {
                    var index = Count;

                    foreach (var itemAdded in list_ItemsAdded_Plan)
                    {
                        var isAdded = _set.Add(itemAdded);

                        Debug.Assert(isAdded);
                    }

                    _list.AddRange(list_ItemsAdded_Plan);

                    OnCollectionChanged(
                        NotifyCollectionChangedAction.Add,
                        list_ItemsAdded_Plan,
                        index);
                }

                if (countChanged)
                {
                    OnCountPropertyChanged();
                }

                OnIndexerPropertyChanged();

                VerifyState();

                deferredEventNotifications.Complete();
            }
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is a subset of other;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool IsSubsetOf(IEnumerable<T> other)
        {
            VerifyState();

            return _set.IsSubsetOf(other);
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> is a proper subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is a proper subset of other;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool IsProperSubsetOf(IEnumerable<T> other)
        {
            VerifyState();

            return _set.IsProperSubsetOf(other);
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> is a superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is a superset of other;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool IsSupersetOf(IEnumerable<T> other)
        {
            VerifyState();

            return _set.IsSupersetOf(other);
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> is a proper superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is a proper superset of other;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool IsProperSupersetOf(IEnumerable<T> other)
        {
            VerifyState();

            return _set.IsProperSupersetOf(other);
        }

        /// <summary>
        ///     Determines whether the current <see cref="ObservableSet{T}" /> object and a specified collection share common elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> and other share at least one common element;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool Overlaps(IEnumerable<T> other)
        {
            VerifyState();

            return _set.Overlaps(other);
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is equal to other;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool SetEquals(IEnumerable<T> other)
        {
            VerifyState();

            return _set.SetEquals(other);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="ObservableSet{T}" /> to an array.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional array that is the destination of the elements copied from the <see cref="ObservableSet{T}" />.
        ///     The array must have zero-based indexing.
        /// </param>
        public virtual void CopyTo(T[] array)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(array);
#else
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
#endif

            VerifyState();

            _list.CopyTo(array);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="ObservableSet{T}" /> to an array, starting at the specified array index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional array that is the destination of the elements copied from the <see cref="ObservableSet{T}" />.
        ///     The array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(array);
#else
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
#endif

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            VerifyState();

            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Copies the specified number of elements of the <see cref="ObservableSet{T}" /> to an array, starting at the specified array index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional array that is the destination of the elements copied from the <see cref="ObservableSet{T}" />.
        ///     The array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <param name="count">The number of elements to copy to array.</param>
        public virtual void CopyTo(T[] array, int arrayIndex, int count)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(array);
#else
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
#endif

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (array.Length - arrayIndex < count)
            {
                throw new ArgumentException(
                    Resources.ObservableSet_DestinationArrayTooSmall,
                    nameof(array));
            }

            VerifyState();

            if (count > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            _list.CopyTo(0, array, arrayIndex, count);
        }

        protected void VerifyState()
        {
            EnsureStorageConsistency();
            AssertStorageInvariants();
        }

        private void EnsureStorageConsistency()
        {
            if (_set.Count != _list.Count)
            {
                throw new InvalidOperationException(
                    string.Format(Resources.ObservableSet_InternalStorageInconsistent,
                                  $"{nameof(_set)}.{nameof(_set.Count)}",
                                  _set.Count,
                                  $"{nameof(_list)}.{nameof(_list.Count)}",
                                  _list.Count));
            }
        }

        [Conditional("DEBUG")]
        private void AssertStorageInvariants()
        {
            Debug.Assert(_set.Count == _list.Count);

            var items = new HashSet<T>(_set.Comparer);

            foreach (var item in _list)
            {
                Debug.Assert(_set.Contains(item));
                Debug.Assert(items.Add(item));
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
                    throw new InvalidOperationException(Resources.ObservableSet_ReentrancyNotAllowed);
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

        /// <summary>
        ///     Defers collection and property change event notifications until the returned operation is completed.
        /// </summary>
        /// <returns>
        ///     An operation that queues event notifications and replays them when completed.
        /// </returns>
        /// <remarks>
        ///     Event notifications raised during the operation are queued and replayed in the order they were raised
        ///     when <see cref="DeferredEventNotificationsOperation.Complete" /> is called.
        ///     If the operation is disposed before completion, queued event notifications are discarded.
        ///     Deferred event notification operations cannot be nested.
        /// </remarks>
        protected DeferredEventNotificationsOperation DeferEventNotifications()
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
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Move" />) event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(object? item, int oldIndex, int newIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        /// <remarks>
        ///     WPF ICollectionView/ListCollectionView consumers require Add and Remove notifications
        ///     to contain the actual item index as observed through this collection's public enumeration order.
        ///     If the supplied index does not match the item position visible through <see cref="IEnumerable{T}" />,
        ///     WPF may throw errors such as "Added item does not appear at given index".
        ///
        ///     The index must be based on <c>_list</c>, because <c>_list</c> defines
        ///     this collection's stable insertion/enumeration order.
        ///     <c>_set</c> is only for uniqueness and membership lookup.
        /// </remarks>
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
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Replace" />) event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(IList oldItems, IList newItems, int startingIndex)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItems, newItems, startingIndex);
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
        ///     Properties/methods modifying this <see cref="ObservableSet{T}" /> will raise
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

        protected void OnCountPropertyChanging()
        {
            OnPropertyChanging(EventArgsCache.CountPropertyChanging);
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanging" /> event.
        /// </summary>
        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            if (_deferredEventNotificationsOperation is not null)
            {
                _deferredEventNotificationsOperation.AddEventArgs(e);

                return;
            }

            if (PropertyChanging is PropertyChangingEventHandler handler)
            {
                handler(this, e);
            }
        }

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
            internal ObservableSet<T> _collection;

            public SimpleMonitor(ObservableSet<T> collection)
            {
                Debug.Assert(collection is not null);

                _collection = collection!;
            }

            public void Dispose()
            {
                _collection._blockReentrancyCount--;
            }
        }

        protected sealed class DeferredEventNotificationsOperation : IDisposable
        {
            private readonly ObservableSet<T> _collection;
            private readonly List<EventArgs> _eventArgsList = [];

            private bool _isDisposed;

            private bool _isCompleted;

            public DeferredEventNotificationsOperation(ObservableSet<T> collection)
            {
#if NET8_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(collection);
#else
                if (collection is null)
                {
                    throw new ArgumentNullException(nameof(collection));
                }
#endif

                if (collection._deferredEventNotificationsOperation is not null)
                {
                    throw new InvalidOperationException(Resources.ObservableSet_NestedDeferredEventNotificationsNotAllowed);
                }

                _collection = collection;
                _collection._deferredEventNotificationsOperation = this;
            }

            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;

                _collection._deferredEventNotificationsOperation = null;

                if (!_isCompleted)
                {
                    _eventArgsList.Clear();
                }
            }

            public void Complete()
            {
#if NET8_0_OR_GREATER
                ObjectDisposedException.ThrowIf(_isDisposed, this);
#else
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(DeferredEventNotificationsOperation));
                }
#endif

                if (_isCompleted)
                {
                    return;
                }

                _isCompleted = true;

                _collection._deferredEventNotificationsOperation = null;

                try
                {
                    foreach (var eventArgs in _eventArgsList)
                    {
                        if (eventArgs is NotifyCollectionChangedEventArgs collectionChangedEventArgs)
                        {
                            _collection.OnCollectionChanged(collectionChangedEventArgs);
                        }
                        else if (eventArgs is PropertyChangingEventArgs propertyChangingEventArgs)
                        {
                            _collection.OnPropertyChanging(propertyChangingEventArgs);
                        }
                        else if (eventArgs is PropertyChangedEventArgs propertyChangedEventArgs)
                        {
                            _collection.OnPropertyChanged(propertyChangedEventArgs);
                        }
                    }
                }
                finally
                {
                    _eventArgsList.Clear();
                }
            }

            public void AddEventArgs<TEventArgs>(TEventArgs eventArgs) where TEventArgs : EventArgs
            {
#if NET8_0_OR_GREATER
                ObjectDisposedException.ThrowIf(_isDisposed, this);
#else
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(DeferredEventNotificationsOperation));
                }
#endif

                if (_isCompleted)
                {
                    throw new InvalidOperationException(Resources.ObservableSet_DeferredEventNotificationsCompleted);
                }

                _eventArgsList.Add(eventArgs);
            }
        }

        internal static class EventArgsCache
        {
            public static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
            public static readonly PropertyChangingEventArgs CountPropertyChanging = new(nameof(Count));
            public static readonly PropertyChangedEventArgs CountPropertyChanged = new(nameof(Count));
            public static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");

            public static readonly T[] Items_Empty = [];
        }
    }
}
