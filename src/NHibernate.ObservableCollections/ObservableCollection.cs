namespace Iesi.Collections.Generic
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    ///     A generic collection that fires events
    ///     when item(s) have been added to or removed from the collection.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the collection.
    /// </typeparam>
    /// <remarks>
    ///     REFERENCES:
    ///     -   https://github.com/dotnet/runtime/issues/18087
    ///     -   https://gist.github.com/weitzhandler/65ac9113e31d12e697cb58cd92601091
    ///     -   https://stackoverflow.com/questions/670577/observablecollection-doesnt-support-addrange-method-so-i-get-notified-for-each
    ///     -   https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/ObjectModel/Collection.cs
    ///     -   https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs
    ///     -   https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs
    ///     -   https://happynomad121.blogspot.com/2007/12/collections-for-wpf-and-nhibernate.html
    ///     -   https://referencesource.microsoft.com/#mscorlib/system/collections/objectmodel/collection.cs
    ///     -   https://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs
    ///     -   https://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
    public class ObservableCollection<T> :
        Collection<T>,
        INotifyCollectionChanged, INotifyPropertyChanged
    {
        private SimpleMonitor? _monitor; // Lazily allocated only when a subclass calls BlockReentrancy() or during serialization. Do not rename (binary serialization)

        [NonSerialized]
        private int _blockReentrancyCount;

        [NonSerialized]
        private DeferredEventsCollection? _deferredEventsCollection;

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
        [field: NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler? PropertyChanged;

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

            var removedItem = this[index];
            base.RemoveItem(index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        /// <summary>
        ///     Called by base class <see cref="Collection{T}" /> when an item is set in collection;
        ///     raises a <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();

            var oldItem = this[index];
            base.SetItem(index, item);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, item, index);
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

            var movedItem = this[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, movedItem);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Move, movedItem, oldIndex, newIndex);
        }

        /// <summary>
        ///     Called by base class <see cref="Collection{T}" /> when the collection is being cleared;
        ///     raises a <see cref="CollectionChanged" /> event to any listeners.
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

        /// <summary>
        ///     Removes a range.
        ///     Raises CollectionChanged (NotifyCollectionChangedAction.Remove).
        /// </summary>
        /// <param name="collection"></param>
        public void RemoveRange(IEnumerable<T> collection)
        {
            RemoveItemsRange(collection);
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
        ///     Replace a range with fewer, equal, or more items.
        ///     Raises CollectionChanged (NotifyCollectionChangedAction.Replace).
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="collection"></param>
        public void ReplaceRange(int index, int count, IEnumerable<T> collection)
        {
            RemoveItemsRange(index, count);
            InsertItemsRange(index, collection);
        }

        protected virtual void InsertItemsRange(int index, IEnumerable<T> collection)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

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

            var addedItems = collection.ToList();
            ((List<T>) Items).InsertRange(index, addedItems);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Add, addedItems, index);
        }

        protected virtual void RemoveItemsRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

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

                if (lastIndex == index && lastCluster != null)
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

        protected virtual void RemoveItemsRange(int index, int count)
        {
            if (index < 0 || index > Count || index + count > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            CheckReentrancy();

            var items = (List<T>) Items;
            var removedItems = items.GetRange(index, count);
            items.RemoveRange(index, count);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            if (Count == 0)
            {
                OnCollectionReset();
            }
            else
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems, index);
            }
        }

        protected virtual IDisposable DeferEvents()
        {
            return new DeferredEventsCollection(this);
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
            if (_deferredEventsCollection != null)
            {
                _deferredEventsCollection.Add(e);

                return;
            }

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
        ///     Helper to raise a <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        ///     Helper to raise a <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int oldIndex, int newIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, newIndex, oldIndex));
        }

        /// <summary>
        ///     Helper to raise a <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? oldItem, object? newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        ///     Helper to raise a ranged <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items, startingIndex));
        }

        /// <summary>
        ///     Helper to raise a ranged <see cref="CollectionChanged" /> event with action == Reset to any listeners.
        /// </summary>
        private void OnCollectionReset()
        {
            OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        }

        /// <summary>
        ///     Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     Helper to raise a PropertyChanged event for the Count property.
        /// </summary>
        private void OnCountPropertyChanged()
        {
            OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        }

        /// <summary>
        ///     Helper to raise a PropertyChanged event for the Indexer property.
        /// </summary>
        private void OnIndexerPropertyChanged()
        {
            OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
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
                if (CollectionChanged?.GetInvocationList().Length > 1)
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
            internal int _busyCount; // Only used during (de)serialization to maintain compatibility with desktop. Do not rename (binary serialization)

            [NonSerialized]
            internal ObservableCollection<T> _collection = null!;

            public SimpleMonitor(ObservableCollection<T> collection)
            {
                Debug.Assert(collection != null);

                _collection = collection!;
            }

            public void Dispose()
            {
                _collection._blockReentrancyCount--;
            }
        }

        private sealed class DeferredEventsCollection : List<NotifyCollectionChangedEventArgs>, IDisposable
        {
            private readonly ObservableCollection<T> _collection;

            public DeferredEventsCollection(ObservableCollection<T> collection)
            {
                Debug.Assert(collection != null);
                Debug.Assert(collection!._deferredEventsCollection == null);

                _collection = collection;
                _collection._deferredEventsCollection = this;
            }

            public void Dispose()
            {
                _collection._deferredEventsCollection = null;
                foreach (var args in this)
                {
                    _collection.OnCollectionChanged(args);
                }
            }
        }

        internal static class EventArgsCache
        {
            internal static readonly PropertyChangedEventArgs CountPropertyChanged = new(nameof(Count));
            internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
            internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
        }
    }
}
