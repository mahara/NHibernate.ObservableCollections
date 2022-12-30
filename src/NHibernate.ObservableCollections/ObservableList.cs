namespace Iesi.Collections.Generic
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     A generic list that fires events
    ///     when item(s) have been added to or removed from the list.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the list.
    /// </typeparam>
    /// <author>Microsoft Corporation</author>
    /// <author>Maximilian Haru Raditya</author>
    /// <remarks>
    ///     <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs" />
    ///     <see href="https://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs" />
    ///     <see href="https://referencesource.microsoft.com/#mscorlib/system/collections/objectmodel/collection.cs" />
    /// </remarks>
    [Serializable]
    [ComVisible(false)]
    [DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableList<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        protected const string CountPropertyName = "Count";

        protected const string IndexerName = "Item[]";

        private readonly SimpleMonitor _monitor = new();

        [NonSerialized]
        private object _syncRoot;

        public ObservableList()
        {
        }

        public ObservableList(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (var item in collection)
            {
                InnerList.Add(item);
            }
        }

        protected IList<T> InnerList { get; } = new List<T>();

        bool ICollection.IsSynchronized =>
            false;

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    if (InnerList is ICollection c)
                    {
                        _syncRoot = c.SyncRoot;
                    }
                    else
                    {
                        Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                    }
                }

                return _syncRoot;
            }
        }

        bool IList.IsFixedSize =>
            ((IList) InnerList).IsFixedSize;

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

                try
                {
                    this[index] = (T) value;
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
                }
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection) InnerList).CopyTo(array, index);
        }

        int IList.Add(object value)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

            try
            {
                Add((T) value);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
            }

            return Count - 1;
        }

        void IList.Insert(int index, object value)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

            try
            {
                Insert(index, (T) value);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
            }
        }

        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
            {
                Remove((T) value);
            }
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T) value);
            }

            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return IndexOf((T) value);
            }

            return -1;
        }

        public int Count =>
            InnerList.Count;

        public bool IsReadOnly =>
            InnerList.IsReadOnly;

        public T this[int index]
        {
            get => InnerList[index];
            set
            {
                if (index < 0 || index >= InnerList.Count)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                SetItem(index, value);
            }
        }

        public virtual void Add(T item)
        {
            CheckReentrancy();

            InnerList.Add(item);

            var index = InnerList.Count - 1;

            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public virtual bool Remove(T item)
        {
            CheckReentrancy();

            var index = InnerList.IndexOf(item);
            var isRemoved = index >= 0;
            if (isRemoved)
            {
                InnerList.RemoveAt(index);

                OnPropertyChanged(CountPropertyName);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }

            return isRemoved;
        }

        public virtual void Clear()
        {
            CheckReentrancy();

            InnerList.Clear();

            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        public bool Contains(T item)
        {
            return InnerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            InnerList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        public virtual void Insert(int index, T item)
        {
            if (index < 0 || index > InnerList.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            CheckReentrancy();

            InnerList.Insert(index, item);

            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public virtual void RemoveAt(int index)
        {
            if (index < 0 || index >= InnerList.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            CheckReentrancy();

            var removedItem = InnerList[index];

            InnerList.RemoveAt(index);

            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        public int IndexOf(T item)
        {
            return InnerList.IndexOf(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) InnerList).GetEnumerator();
        }

        /// <summary>
        ///     Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        /// <remarks>
        ///     see <seealso cref="INotifyCollectionChanged" />
        /// </remarks>
        [field: NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual void AddRange(IEnumerable<T> items)
        {
            // Add items starting at the last item position by default.
            var startingIndex = InnerList.Count;

            AddRange(startingIndex, items);
        }

        public virtual void AddRange(int startingIndex, IEnumerable<T> items)
        {
            if (startingIndex < 0 || startingIndex > InnerList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startingIndex));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            CheckReentrancy();

            var addedItems = items.ToArray();
            foreach (var item in addedItems)
            {
                InnerList.Add(item);
            }

            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, addedItems, startingIndex);
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            CheckReentrancy();

            var removedItems = new List<T>();
            foreach (var item in items)
            {
                if (InnerList.Remove(item))
                {
                    removedItems.Add(item);
                }
            }

            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems.ToArray(), 0);
        }

        /// <summary>
        ///     Move item at oldIndex to newIndex
        ///     to reorder an item in the collection.
        /// </summary>
        /// <param name="oldIndex">
        ///     The old index.
        /// </param>
        /// <param name="newIndex">
        ///     The new index.
        /// </param>
        public virtual void Move(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex > InnerList.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            if (newIndex < 0 || newIndex > InnerList.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            CheckReentrancy();

            var movedItem = this[oldIndex];

            InnerList.RemoveAt(oldIndex);
            InnerList.Insert(newIndex, movedItem);

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, movedItem, newIndex, oldIndex);
        }

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>.
            return value is T || (value == null && default(T) == null);
        }

        protected virtual void SetItem(int index, T value)
        {
            CheckReentrancy();

            var originalItem = this[index];
            InnerList[index] = value;

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, value, index);
        }

        /// <summary>
        ///     Checks if there is currently no reentrancy
        ///     that is making any changes to this set/list.
        ///     -- ORIGINAL SUMMARY --
        ///     Check and assert for reentrant attempts to change this collection.
        /// </summary>
        /// <remarks>
        ///     This should be done before making any changes to this set/list.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     raised when changing the collection
        ///     while another collection change is still being notified to other listeners
        /// </exception>
        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                var handler = CollectionChanged;
                if (handler != null && handler.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Cannot change ObservableList during a CollectionChanged event.");
                }
            }
        }

        /// <summary>
        ///     Blocks new reentrancy to prevent any changes to this set/collection.
        ///     -- ORIGINAL SUMMARY --
        ///     Disallow reentrant attempts to change this collection. E.g. a event handler
        ///     of the CollectionChanged event is not allowed to make changes to this collection.
        /// </summary>
        /// <remarks>
        ///     typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
        ///     <code>
        ///         using (BlockReentrancy())
        ///         {
        ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
        ///         }
        ///     </code>
        /// </remarks>
        /// <returns>
        ///     A <see cref="SimpleMonitor" /> that blocks new reentrancy.
        /// </returns>
        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();

            return _monitor;
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<T> items, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, (IList) items, index));
        }

        protected void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                using (BlockReentrancy())
                {
                    handler(this, e);
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
