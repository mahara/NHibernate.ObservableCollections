using System.Diagnostics;

namespace Iesi.Collections.Generic
{
    /// <summary>
    ///     Represents a dynamic data set that provides notifications
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
        private SimpleMonitor? _monitor; // Lazily allocated only when a subclass calls BlockReentrancy() or during serialization. Do not rename (binary serialization).

        [NonSerialized]
        private int _blockReentrancyCount;

        private HashSet<T> _set;
        private List<T> _list;

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
            _set = new HashSet<T>(collection, comparer);
            _list = [.. _set];
        }

        /// <summary>
        ///     Occurs when the contents of the <see cref="ObservableSet{T}" /> changes.
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        ///     Occurs when a property of this <see cref="ObservableSet{T}" /> (such as <see cref="Count" />) is changing.
        /// </summary>
        public virtual event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        ///     Occurs when a property of this <see cref="ObservableSet{T}" /> (such as <see cref="Count" />) changes.
        /// </summary>
        public virtual event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Gets the number of elements that are contained in the <see cref="ObservableSet{T}" />.
        /// </summary>
        public virtual int Count => _set.Count;

        /// <summary>
        ///     Gets a value indicating whether the <see cref="ObservableSet{T}" /> is read-only.
        /// </summary>
        public virtual bool IsReadOnly => ((ICollection<T>) _set).IsReadOnly;

        /// <summary>
        ///     Gets the <see cref="IEqualityComparer{T}" /> object that is used to determine equality for the values in the set.
        /// </summary>
        public virtual IEqualityComparer<T> Comparer => _set.Comparer;

        /// <summary>
        ///     Gets the element at the specified index in the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the <see cref="ObservableSet{T}" />.</returns>
        public T this[int index] => _list[index];

        /// <summary>
        ///     Returns an enumerator that iterates through the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <returns>
        ///     An enumerator for the <see cref="ObservableSet{T}" />.
        /// </returns>
        public virtual HashSet<T>.Enumerator GetEnumerator()
        {
            return _set.GetEnumerator();
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
        ///     Raises a <see cref="CollectionChanged" />'s <see cref="NotifyCollectionChangedAction.Reset" /> event to any listeners.
        /// </summary>
        public void Refresh()
        {
            RefreshItems();
        }

        /// <summary>
        ///     Raises a <see cref="CollectionChanged" />'s <see cref="NotifyCollectionChangedAction.Reset" /> event to any listeners.
        /// </summary>
        protected virtual void RefreshItems()
        {
            OnCountPropertyChanged();
            OnCollectionReset();
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> object contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in the <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> contains the specified element; otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool Contains(T item)
        {
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
        public virtual int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        /// <summary>
        ///     Adds the specified element to the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is added to the <see cref="ObservableSet{T}" />; <see langword="false" /> if the element is already present.
        /// </returns>
        public virtual bool Add(T item)
        {
            CheckReentrancy();

            var isAdded = _set.Add(item);
            if (isAdded)
            {
                OnCountPropertyChanging();

                _list.Add(item);

                var index = _set.Count - 1;

                OnCountPropertyChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            }

            return isAdded;
        }

        /// <inheritdoc />
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        ///     Removes the specified element from the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool Remove(T item)
        {
            CheckReentrancy();

            var isRemoved = _set.Remove(item);
            if (isRemoved)
            {
                OnCountPropertyChanging();

                var index = _list.IndexOf(item);

                _list.Remove(item);

                OnCountPropertyChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }

            return isRemoved;
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
        public virtual int RemoveWhere(Predicate<T> match)
        {
            if (_set.Count == 0)
            {
                return 0;
            }

            CheckReentrancy();

            var copy = new HashSet<T>(_set, _set.Comparer);

            var removedCount = copy.RemoveWhere(match);

            if (removedCount == 0)
            {
                return 0;
            }

            var removed = _set.Where(i => !copy.Contains(i)).ToArray();

            OnCountPropertyChanging();

            _set = copy;
            _list = [.. _set];

            OnCountPropertyChanged();
            OnCollectionChanged(removed, EventArgsCache.Items_Empty, 0);

            return removedCount;
        }

        /// <summary>
        ///     Removes all elements from the <see cref="ObservableSet{T}" />.
        /// </summary>
        public virtual void Clear()
        {
            CheckReentrancy();

            if (_set.Count == 0)
            {
                return;
            }

            OnCountPropertyChanging();

            _set.Clear();
            _list.Clear();

            OnCountPropertyChanged();
            OnCollectionReset();
        }

        /// <summary>
        ///     Sets the capacity of the <see cref="ObservableSet{T}" /> to the actual number of elements it contains,
        ///     rounded up to a nearby, implementation-specific value.
        /// </summary>
        public virtual void TrimExcess()
        {
            _set.TrimExcess();
        }

        /// <summary>
        ///     Modifies the <see cref="ObservableSet{T}" /> to contain all elements that are present in itself, the specified collection, or both.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        public virtual void UnionWith(IEnumerable<T> other)
        {
            CheckReentrancy();

            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.UnionWith(other);

            if (copy.Count == _set.Count)
            {
                return;
            }

            var added = copy.Where(i => !_set.Contains(i)).ToArray();

            OnCountPropertyChanging();

            _set = copy;
            _list = [.. _set];

            OnCountPropertyChanged();
            OnCollectionChanged(EventArgsCache.Items_Empty, added, 0);
        }

        /// <summary>
        ///     Modifies the current <see cref="ObservableSet{T}" /> to contain only elements
        ///     that are present in that object and in the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        public virtual void IntersectWith(IEnumerable<T> other)
        {
            CheckReentrancy();

            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.IntersectWith(other);

            if (copy.Count == _set.Count)
            {
                return;
            }

            var removed = _set.Where(i => !copy.Contains(i)).ToArray();

            OnCountPropertyChanging();

            _set = copy;
            _list = [.. _set];

            OnCountPropertyChanged();
            OnCollectionChanged(removed, EventArgsCache.Items_Empty, 0);
        }

        /// <summary>
        ///     Removes all elements in the specified collection from the <see cref="ObservableSet{T}" />.
        /// </summary>
        /// <param name="other">The collection of items to remove from the current <see cref="ObservableSet{T}" />.</param>
        public virtual void ExceptWith(IEnumerable<T> other)
        {
            CheckReentrancy();

            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.ExceptWith(other);

            if (copy.Count == _set.Count)
            {
                return;
            }

            var removed = _set.Where(i => !copy.Contains(i)).ToArray();

            OnCountPropertyChanging();

            _set = copy;
            _list = [.. _set];

            OnCountPropertyChanged();
            OnCollectionChanged(removed, EventArgsCache.Items_Empty, 0);
        }

        /// <summary>
        ///     Modifies the current <see cref="ObservableSet{T}" /> to contain only elements that are present either in that object
        ///     or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        public virtual void SymmetricExceptWith(IEnumerable<T> other)
        {
            CheckReentrancy();

            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.SymmetricExceptWith(other);

            var removed = _set.Where(i => !copy.Contains(i)).ToArray();
            var added = copy.Where(i => !_set.Contains(i)).ToArray();

            if (removed.Length == 0 && added.Length == 0)
            {
                return;
            }

            OnCountPropertyChanging();

            _set = copy;
            _list = [.. _set];

            OnCountPropertyChanged();
            OnCollectionChanged(removed, added, 0);
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is a subset of other; otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> is a proper subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is a proper subset of other; otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> is a superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is a superset of other; otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> is a proper superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is a proper superset of other; otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        /// <summary>
        ///     Determines whether the current <see cref="ObservableSet{T}" /> object and a specified collection share common elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> and other share at least one common element; otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        /// <summary>
        ///     Determines whether the <see cref="ObservableSet{T}" /> and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="ObservableSet{T}" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObservableSet{T}" /> is equal to other; otherwise, <see langword="false" />.
        /// </returns>
        public virtual bool SetEquals(IEnumerable<T> other)
        {
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
            _set.CopyTo(array);
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
            _set.CopyTo(array, arrayIndex);
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
            _set.CopyTo(array, arrayIndex, count);
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
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Reset" />) event to any listeners.
        /// </summary>
        protected void OnCollectionReset()
        {
            OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        [Obsolete("Use OnCollectionChanged(NotifyCollectionChangedAction, object?, int) instead.")]
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object? item)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
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
        [Obsolete("Use OnCollectionChanged(IList, IList, int) instead.")]
        protected void OnCollectionChanged(IList oldItems, IList newItems)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems));
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(IList oldItems, IList newItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, startingIndex));
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
            CollectionChanged?.Invoke(this, e);
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
            PropertyChanging?.Invoke(this, e);
        }

        protected void OnCountPropertyChanged()
        {
            OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
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

        internal static class EventArgsCache
        {
            public static readonly T[] Items_Empty = [];

            public static readonly PropertyChangingEventArgs CountPropertyChanging = new(nameof(Count));
            public static readonly PropertyChangedEventArgs CountPropertyChanged = new(nameof(Count));
            public static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
        }
    }
}
