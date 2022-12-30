namespace Iesi.Collections.Generic
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     A generic set that fires events
    ///     when item(s) have been added to or removed from the set.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the set.
    /// </typeparam>
    /// <author>Adrian Alexander</author>
    /// <author>Microsoft Corporation</author>
    /// <author>Maximilian Haru Raditya</author>
    /// <remarks>
    ///     <see href="http://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs" />
    /// </remarks>
    [Serializable]
    [ComVisible(false)]
    [DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableSet<T> : ISet<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        protected const string CountPropertyName = "Count";

        private readonly SimpleMonitor _monitor = new();

        public ObservableSet()
        {
        }

        public ObservableSet(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            Initialize(collection);
        }

        protected ISet<T> InnerSet { get; } = new HashSet<T>();

        protected IList<T> InnerList { get; } = new List<T>();

        /// <summary>
        ///     Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        /// <remarks>
        ///     see <seealso cref="T:System.Collections.Specialized.INotifyCollectionChanged" />
        /// </remarks>
        /// <inheritdoc />
        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        ///     PropertyChanged event (per <see cref="T:System.ComponentModel.INotifyPropertyChanged" />).
        /// </summary>
        /// <inheritdoc />
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        [field: NonSerialized]
        protected event PropertyChangedEventHandler? PropertyChanged;

        public int Count => InnerSet.Count;

        public bool IsReadOnly => InnerSet.IsReadOnly;

        public IEnumerator<T> GetEnumerator()
        {
            return InnerSet.GetEnumerator();
        }

        public virtual bool Add(T item)
        {
            CheckReentrancy();

            EnsureConsistency();

            var isAdded = InnerSet.Add(item);
            if (isAdded)
            {
                InnerList.Add(item);

                EnsureConsistency();

                var index = InnerSet.Count - 1;

                OnPropertyChanged(CountPropertyName);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item!, index);
            }

            return isAdded;
        }

        public virtual void UnionWith(IEnumerable<T> other)
        {
            InnerSet.UnionWith(other);

            ReinitializeItems();
        }

        public virtual void IntersectWith(IEnumerable<T> other)
        {
            InnerSet.IntersectWith(other);

            ReinitializeItems();
        }

        public virtual void ExceptWith(IEnumerable<T> other)
        {
            InnerSet.ExceptWith(other);

            ReinitializeItems();
        }

        public virtual void SymmetricExceptWith(IEnumerable<T> other)
        {
            InnerSet.SymmetricExceptWith(other);

            ReinitializeItems();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return InnerSet.IsSubsetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return InnerSet.IsProperSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return InnerSet.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return InnerSet.IsProperSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return InnerSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return InnerSet.SetEquals(other);
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        bool ICollection<T>.Remove(T item)
        {
            return Remove(item);
        }

        void ICollection<T>.Clear()
        {
            Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) InnerSet).GetEnumerator();
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            // Add items starting at the last item position by default.
            var startingIndex = InnerSet.Count;

            AddRange(startingIndex, items);
        }

        public virtual void AddRange(int startingIndex, IEnumerable<T> items)
        {
            if (startingIndex < 0 || startingIndex > InnerSet.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startingIndex));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            CheckReentrancy();

            EnsureConsistency();

            var addedItems = new HashSet<T>();
            foreach (var item in items)
            {
                if (InnerSet.Add(item))
                {
                    InnerList.Add(item);

                    EnsureConsistency();

                    addedItems.Add(item);
                }
            }

            EnsureConsistency();

            if (addedItems.Count > 0)
            {
                OnPropertyChanged(CountPropertyName);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, addedItems, startingIndex);
            }
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            CheckReentrancy();

            EnsureConsistency();

            var removedItems = new List<T>();
            foreach (var item in items)
            {
                if (InnerSet.Remove(item))
                {
                    InnerList.Remove(item);

                    EnsureConsistency();

                    removedItems.Add(item);
                }
            }

            EnsureConsistency();

            if (removedItems.Count > 0)
            {
                OnPropertyChanged(CountPropertyName);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems.ToArray(), 0);
            }
        }

        public virtual bool Remove(T item)
        {
            CheckReentrancy();

            EnsureConsistency();

            var index = InnerList.IndexOf(item);
            var isRemoved = index >= 0 && InnerSet.Remove(item);
            if (isRemoved)
            {
                InnerList.RemoveAt(index);

                EnsureConsistency();

                OnPropertyChanged(CountPropertyName);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item!, index);
            }

            return isRemoved;
        }

        public virtual void Clear()
        {
            CheckReentrancy();

            EnsureConsistency();

            InnerSet.Clear();
            InnerList.Clear();

            EnsureConsistency();

            OnPropertyChanged(CountPropertyName);
            OnCollectionReset();
        }

        public bool Contains(T item)
        {
            return InnerSet.Contains(item);
        }

        public int IndexOf(T item)
        {
            return InnerList.IndexOf(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            InnerSet.CopyTo(array, arrayIndex);
        }

        private void Initialize(IEnumerable<T> collection)
        {
            EnsureConsistency();

            foreach (var item in collection)
            {
                if (InnerSet.Add(item))
                {
                    InnerList.Add(item);

                    EnsureConsistency();
                }
            }
        }

        /// <summary>
        ///     Checks if there is currently no reentrancy that is making any changes to this set/list.
        ///     -- ORIGINAL SUMMARY --
        ///     Check and assert for reentrant attempts to change this collection.
        /// </summary>
        /// <remarks>
        ///     This should be done before making any changes to this set/list.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     Raised when changing the collection
        ///     while another collection change is still being notified to other listeners.
        /// </exception>
        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                // We can allow changes if there's only one listener.
                // The problem only arises if reentrant changes make
                // the original event args invalid for later listeners.
                // This keeps existing code working (e.g. Selector.SelectedItems).
                var handler = CollectionChanged;
                if (handler != null && handler.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException($"Cannot change {nameof(ObservableSet<T>)} during a {nameof(CollectionChanged)} event.");
                }
            }
        }

        /// <summary>
        ///     Blocks new reentrancy to prevent any changes to this set/collection.
        ///     -- ORIGINAL SUMMARY --
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
        /// <returns>
        ///     A <see cref="SimpleMonitor" /> that blocks new reentrancy.
        /// </returns>
        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();

            return _monitor;
        }

        protected virtual void EnsureConsistency()
        {
            Contract.Assert(InnerList.Count == InnerSet.Count, "Internal data inconsistent.");
        }

        protected virtual void ReinitializeItems()
        {
            InnerList.Clear();

            ((List<T>) InnerList).AddRange(InnerSet);

            EnsureConsistency();

            OnCollectionReset();
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, startingIndex));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<T> items, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, (IList) items, startingIndex));
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

        protected void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
