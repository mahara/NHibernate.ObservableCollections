namespace Iesi.Collections.Generic
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    ///     A generic list that fires events when item(s) have been added to or removed from the list.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the list.
    /// </typeparam>
    /// <remarks>
    ///     REFERENCES:
    ///     -   https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/ObjectModel/Collection.cs
    ///     -   https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs
    ///     -   https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs
    ///     -   https://github.com/dotnet/runtime/issues/18087
    ///     -   https://referencesource.microsoft.com/#mscorlib/system/collections/objectmodel/collection.cs
    ///     -   https://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
    public class ObservableList<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private SimpleMonitor? _monitor; // Lazily allocated only when a subclass calls BlockReentrancy() or during serialization. Do not rename (binary serialization)

        [NonSerialized]
        private int _blockReentrancyCount;

        /// <summary>
        ///     Initializes a new instance of ObservableList that is empty and has default initial capacity.
        /// </summary>
        public ObservableList()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the ObservableList class that contains elements
        ///     copied from the specified collection and has sufficient capacity
        ///     to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <remarks>
        ///     The elements are copied onto the ObservableList
        ///     in the same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> collection is a null reference </exception>
        public ObservableList(IEnumerable<T> collection) :
            base(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection))))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the ObservableList class
        ///     that contains elements copied from the specified list
        /// </summary>
        /// <param name="list">The list whose elements are copied to the new list.</param>
        /// <remarks>
        ///     The elements are copied onto the ObservableList
        ///     in the same order they are read by the enumerator of the list.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> list is a null reference </exception>
        public ObservableList(List<T> list) :
            base(new List<T>(list ?? throw new ArgumentNullException(nameof(list))))
        {
        }

        /// <summary>
        ///     Move item at oldIndex to newIndex.
        /// </summary>
        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }


        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        /// <summary>
        ///     Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        [field: NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        ///     Called by base class Collection{T} when an item is added to list;
        ///     raises a CollectionChanged event to any listeners.
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
        ///     Called by base class Collection{T} when an item is removed from list;
        ///     raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            CheckReentrancy();

            var removedItem = this[index];

            base.RemoveItem(index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        /// <summary>
        ///     Called by base class Collection{T} when an item is set in list;
        ///     raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();

            var originalItem = this[index];
            base.SetItem(index, item);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

        /// <summary>
        ///     Called by base class Collection{T} when an item is to be moved within the list;
        ///     raises a CollectionChanged event to any listeners.
        /// </summary>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            var removedItem = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }

        /// <summary>
        ///     Called by base class Collection{T} when the list is being cleared;
        ///     raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
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
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            InsertItemsRange(0, items);
        }

        /// <summary>
        ///     Inserts a range.
        ///     Raises CollectionChanged (NotifyCollectionChangedAction.Add).
        /// </summary>
        /// <param name="index"></param>
        /// <param name="items"></param>
        public void InsertRange(int index, IEnumerable<T> items)
        {
            InsertItemsRange(index, items);
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

        /// <summary>
        ///     Removes a range.
        ///     Raises CollectionChanged (NotifyCollectionChangedAction.Remove).
        /// </summary>
        /// <param name="items"></param>
        public void RemoveRange(IEnumerable<T> items)
        {
            RemoveItemsRange(items);
        }

        /// <summary>
        ///     Replace a range with fewer, equal, or more items.
        ///     Raises CollectionChanged (NotifyCollectionChangedAction.Replace).
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="items"></param>
        public void ReplaceRange(int index, int count, IEnumerable<T> items)
        {
            RemoveItemsRange(index, count);
            InsertItemsRange(index, items);
        }

        protected virtual void InsertItemsRange(int index, IEnumerable<T> items)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            CheckReentrancy();

            var addedItems = items;
            foreach (var item in addedItems)
            {
                Add(item);
            }

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Add, addedItems, index);
        }

        protected virtual void RemoveItemsRange(int index, int count)
        {
            if (index < 0 || index > Count || index + count > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            CheckReentrancy();

            var removedItems = new List<T>();
            var removedItemsCount = 0;
            while (removedItemsCount < count)
            {
                var removedItem = Items[index];
                RemoveAt(index);
                removedItems.Add(removedItem);

                removedItemsCount++;
            }

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems, index);
        }

        protected virtual void RemoveItemsRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            CheckReentrancy();

            var removedItems = new List<T>();
            foreach (var item in items)
            {
                if (Remove(item))
                {
                    removedItems.Add(item);
                }
            }

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems, 0);
        }

        /// <summary>
        ///     Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Raise CollectionChanged event to any listeners.
        ///     Properties/methods modifying this ObservableList will raise
        ///     a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        ///     When overriding this method, either call its base implementation
        ///     or call <see cref="BlockReentrancy" /> to guard against reentrant collection changes.
        /// </remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
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
        ///     Disallow reentrant attempts to change this collection. E.g. an event handler
        ///     of the CollectionChanged event is not allowed to make changes to this collection.
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
                if (CollectionChanged?.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException(SR.ObservableCollectionReentrancyNotAllowed);
                }
            }
        }

        /// <summary>
        ///     Helper to raise a PropertyChanged event for the Count property.
        /// </summary>
        private void OnCountPropertyChanged()
        {
            OnPropertyChanged(ObservableListCache.CountPropertyChanged);
        }

        /// <summary>
        ///     Helper to raise a PropertyChanged event for the Indexer property.
        /// </summary>
        private void OnIndexerPropertyChanged()
        {
            OnPropertyChanged(ObservableListCache.IndexerPropertyChanged);
        }

        /// <summary>
        ///     Helper to raise CollectionChanged event to any listeners.
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        ///     Helper to raise CollectionChanged event to any listeners.
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        ///     Helper to raise CollectionChanged event to any listeners.
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? oldItem, object? newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        ///     Helper to raise CollectionChanged event with action == Reset to any listeners.
        /// </summary>
        private void OnCollectionReset()
        {
            OnCollectionChanged(ObservableListCache.ResetCollectionChanged);
        }

        private SimpleMonitor EnsureMonitorInitialized()
        {
            return _monitor ??= new SimpleMonitor(this);
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
            if (_monitor != null)
            {
                _blockReentrancyCount = _monitor._busyCount;
                _monitor._collection = this;
            }
        }

        // This class helps prevent reentrant calls.
        [Serializable]
        private sealed class SimpleMonitor : IDisposable
        {
            internal int _busyCount; // Only used during (de)serialization to maintain compatibility with desktop. Do not rename (binary serialization)

            [NonSerialized]
            internal ObservableList<T> _collection = null!;

            public SimpleMonitor(ObservableList<T> collection)
            {
                Debug.Assert(collection != null);
                _collection = collection!;
            }

            public void Dispose()
            {
                _collection._blockReentrancyCount--;
            }
        }
    }

    internal static class ObservableListCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new(nameof(ObservableList<object>.Count));
        internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
    }
}
