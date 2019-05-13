namespace Iesi.Collections.Generic
{
    #region Using Directives

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    #endregion

    /// <summary>
    ///     A generic list that fires events when item(s)
    ///     have been added to or removed from the list.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the list.
    /// </typeparam>
    /// <author>Maximilian Haru Raditya</author>
    /// <author>Microsoft Corporation</author>
    /// <remarks>
    ///     <see href="http://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs" />
    ///     <see href="http://referencesource.microsoft.com/#mscorlib/system/collections/objectmodel/collection.cs" />
    /// </remarks>
    [Serializable]
    [ComVisible(false)]
    [DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
    // ReSharper disable once UseNameofExpression
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableList<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        protected const string CountPropertyName = "Count";

        protected const string IndexerName = "Item[]";

        private readonly SimpleMonitor _monitor = new SimpleMonitor();

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
                this.InnerList.Add(item);
            }
        }

        protected IList<T> InnerList { get; } = new List<T>();

        bool ICollection.IsSynchronized =>
            false;

        object ICollection.SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    if (this.InnerList is ICollection c)
                    {
                        this._syncRoot = c.SyncRoot;
                    }
                    else
                    {
                        Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
                    }
                }

                return this._syncRoot;
            }
        }

        bool IList.IsFixedSize =>
            ((IList) this.InnerList).IsFixedSize;

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
            ((ICollection) this.InnerList).CopyTo(array, index);
        }

        int IList.Add(object value)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

            try
            {
                this.Add((T) value);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
            }

            return this.Count - 1;
        }

        void IList.Insert(int index, object value)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

            try
            {
                this.Insert(index, (T) value);
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
                this.Remove((T) value);
            }
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return this.Contains((T) value);
            }

            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return this.IndexOf((T) value);
            }

            return -1;
        }

        public int Count =>
            this.InnerList.Count;

        public bool IsReadOnly =>
            this.InnerList.IsReadOnly;

        public T this[int index]
        {
            get => this.InnerList[index];
            set
            {
                if (index < 0 || index >= this.InnerList.Count)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                this.SetItem(index, value);
            }
        }

        public virtual void Add(T item)
        {
            this.CheckReentrancy();

            this.InnerList.Add(item);

            var index = this.InnerList.Count - 1;

            this.OnPropertyChanged(CountPropertyName);
            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public virtual bool Remove(T item)
        {
            this.CheckReentrancy();

            var index = this.InnerList.IndexOf(item);
            var isRemoved = index >= 0;
            if (isRemoved)
            {
                this.InnerList.RemoveAt(index);

                this.OnPropertyChanged(CountPropertyName);
                this.OnPropertyChanged(IndexerName);
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }

            return isRemoved;
        }

        public virtual void Clear()
        {
            this.CheckReentrancy();

            this.InnerList.Clear();

            this.OnPropertyChanged(CountPropertyName);
            this.OnPropertyChanged(IndexerName);
            this.OnCollectionReset();
        }

        public bool Contains(T item)
        {
            return this.InnerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.InnerList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        public virtual void Insert(int index, T item)
        {
            if (index < 0 || index > this.InnerList.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            this.CheckReentrancy();

            this.InnerList.Insert(index, item);

            this.OnPropertyChanged(CountPropertyName);
            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public virtual void RemoveAt(int index)
        {
            if (index < 0 || index >= this.InnerList.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            this.CheckReentrancy();

            var removedItem = this.InnerList[index];

            this.InnerList.RemoveAt(index);

            this.OnPropertyChanged(CountPropertyName);
            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        public int IndexOf(T item)
        {
            return this.InnerList.IndexOf(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) this.InnerList).GetEnumerator();
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
            add => this.PropertyChanged += value;
            remove => this.PropertyChanged -= value;
        }

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual void AddRange(IEnumerable<T> items)
        {
            // Add items starting at the last item position by default.
            var startingIndex = this.InnerList.Count;

            this.AddRange(startingIndex, items);
        }

        public virtual void AddRange(int startingIndex, IEnumerable<T> items)
        {
            if (startingIndex < 0 || startingIndex > this.InnerList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startingIndex));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.CheckReentrancy();

            var addedItems = items.ToArray();
            foreach (var item in addedItems)
            {
                this.InnerList.Add(item);
            }

            this.OnPropertyChanged(CountPropertyName);
            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, addedItems, startingIndex);
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.CheckReentrancy();

            var removedItems = new List<T>();
            foreach (var item in items)
            {
                if (this.InnerList.Remove(item))
                {
                    removedItems.Add(item);
                }
            }

            this.OnPropertyChanged(CountPropertyName);
            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems.ToArray(), 0);
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
            if (oldIndex < 0 || oldIndex > this.InnerList.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            if (newIndex < 0 || newIndex > this.InnerList.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            this.CheckReentrancy();

            var movedItem = this[oldIndex];

            this.InnerList.RemoveAt(oldIndex);
            this.InnerList.Insert(newIndex, movedItem);

            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Move, movedItem, newIndex, oldIndex);
        }

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>.
            return value is T || value == null && default(T) == null;
        }

        protected virtual void SetItem(int index, T value)
        {
            this.CheckReentrancy();

            var originalItem = this[index];
            this.InnerList[index] = value;

            this.OnPropertyChanged(IndexerName);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, value, index);
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
            if (this._monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                var handler = this.CollectionChanged;
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
            this._monitor.Enter();

            return this._monitor;
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<T> items, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, (IList) items, index));
        }

        protected void OnCollectionReset()
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = this.CollectionChanged;
            if (handler != null)
            {
                using (this.BlockReentrancy())
                {
                    handler(this, e);
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}