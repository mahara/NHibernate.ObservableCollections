using System.Diagnostics;

namespace Iesi.Collections.Generic
{
    /// <summary>
    ///     Represents a dynamic data list that provides notifications
    ///     when items get added or removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the list.
    /// </typeparam>
    /// <remarks>
    ///     AUTHORS:
    ///     -   Microsoft Corporation
    ///     -   Maximilian Haru Raditya
    ///     -   Adrian Alexander
    ///     REFERENCES:
    ///     -   <see href="https://github.com/dotnet/runtime/issues/18087" />
    ///     -   <see href="https://gist.github.com/weitzhandler/65ac9113e31d12e697cb58cd92601091" />
    ///         -   <see href="https://stackoverflow.com/questions/670577/observablecollection-doesnt-support-addrange-method-so-i-get-notified-for-each" />
    ///     -   <see href="https://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html" />
    ///     -   <see href="https://happynomad121.blogspot.com/2007/12/collections-for-wpf-and-nhibernate.html" />
    ///     -   <see href="https://happynomad121.blogspot.com/2008/05/revisiting-bidirectional-assoc-helpers.html" />
    ///     -   <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/ObjectModel/Collection.cs" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs" />
    ///     -   <see href="https://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs" />
    ///     -   <see href="https://referencesource.microsoft.com/#mscorlib/system/collections/objectmodel/collection.cs" />
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableList<T> :
        IList<T>, IList,
        INotifyCollectionChanged, INotifyPropertyChanged
    {
        protected const string CountPropertyName = "Count";

        protected const string IndexerName = "Item[]";

        private readonly SimpleMonitor _monitor = new();

        [NonSerialized]
        private object _syncRoot = null!;

        public ObservableList()
        {
        }

        public ObservableList(IEnumerable<T> collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (var item in collection)
            {
                InnerList.Add(item);
            }
        }

        /// <summary>
        ///     Occurs when an item is added, removed, or moved, or the entire collection is refreshed.
        /// </summary>
        [field: NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        protected IList<T> InnerList { get; } = new List<T>();

        public int Count => InnerList.Count;

        public bool IsReadOnly => InnerList.IsReadOnly;

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

        object? IList.this[int index]
        {
            get => this[index];
            set
            {
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

                try
                {
                    this[index] = (T) value!;
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value!, typeof(T));
                }
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot is null)
                {
                    if (InnerList is ICollection c)
                    {
                        _syncRoot = c.SyncRoot;
                    }
                    else
                    {
                        Interlocked.CompareExchange<object>(ref _syncRoot!, new object(), null!);
                    }
                }

                return _syncRoot;
            }
        }

        bool ICollection.IsSynchronized => false;

        bool IList.IsFixedSize => ((IList) InnerList).IsFixedSize;

        public IEnumerator<T> GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) InnerList).GetEnumerator();
        }

        public bool Contains(T item)
        {
            return InnerList.Contains(item);
        }

        bool IList.Contains(object? value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T) value!);
            }

            return false;
        }

        public int IndexOf(T item)
        {
            return InnerList.IndexOf(item);
        }

        int IList.IndexOf(object? value)
        {
            if (IsCompatibleObject(value))
            {
                return IndexOf((T) value!);
            }

            return -1;
        }

        protected virtual void SetItem(int index, T item)
        {
            CheckReentrancy();

            var oldItem = this[index]!;

            InnerList[index] = item;

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(oldItem, item, index);
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

        int IList.Add(object? value)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

            try
            {
                Add((T) value!);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(value!, typeof(T));
            }

            return Count - 1;
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

        void IList.Insert(int index, object? value)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

            try
            {
                Insert(index, (T) value!);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(value!, typeof(T));
            }
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

        void IList.Remove(object? value)
        {
            if (IsCompatibleObject(value))
            {
                Remove((T) value!);
            }
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

            var movedItem = this[oldIndex]!;

            InnerList.RemoveAt(oldIndex);
            InnerList.Insert(newIndex, movedItem);

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(movedItem, oldIndex, newIndex);
        }

        public virtual void Clear()
        {
            CheckReentrancy();

            InnerList.Clear();

            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

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

            if (items is null)
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
            if (items is null)
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
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems, 0);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            InnerList.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection) InnerList).CopyTo(array, index);
        }

        private static bool IsCompatibleObject(object? value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>.
            return value is T || (value is null && default(T) is null);
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
            if (_monitor.Busy)
            {
                // We can allow changes if there's only one listener.
                // The problem only arises if reentrant changes make the original event args invalid for later listeners.
                // This keeps existing code working (e.g. Selector.SelectedItems).
                var handler = CollectionChanged;
                if (handler is not null &&
                    handler.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Cannot change ObservableList during a CollectionChanged event.");
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
            _monitor.Enter();

            return _monitor;
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> (<see cref="NotifyCollectionChangedAction.Reset" />) event to any listeners.
        /// </summary>
        protected void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
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
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items, startingIndex));
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        ///     Properties/methods modifying this <see cref="ObservableList{T}" /> will raise
        ///     a <see cref="CollectionChanged" /> event through this virtual method.
        /// </summary>
        /// <remarks>
        ///     When overriding this method, either call its base implementation
        ///     or call <see cref="BlockReentrancy" /> to guard against reentrant collection changes.
        /// </remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler is not null)
            {
                using (BlockReentrancy())
                {
                    handler(this, e);
                }
            }
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event for the the specified property name.
        /// </summary>
        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
