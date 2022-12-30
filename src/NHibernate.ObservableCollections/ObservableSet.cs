using System.Diagnostics;
using System.Diagnostics.Contracts;

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
    ///     -   <see href="https://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs" />
    ///     -   <see href="https://referencesource.microsoft.com/#mscorlib/system/collections/objectmodel/collection.cs" />
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableSet<T> :
        ISet<T>,
        INotifyCollectionChanged, INotifyPropertyChanged
    {
        protected const string CountPropertyName = "Count";

        private readonly SimpleMonitor _monitor = new();

        public ObservableSet()
        {
        }

        public ObservableSet(IEnumerable<T> collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            Initialize(collection);
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

        protected ISet<T> InnerSet { get; } = new HashSet<T>();

        protected IList<T> InnerList { get; } = new List<T>();

        public int Count => InnerSet.Count;

        public bool IsReadOnly => InnerSet.IsReadOnly;

        public IEnumerator<T> GetEnumerator()
        {
            return InnerSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) InnerSet).GetEnumerator();
        }

        public bool Contains(T item)
        {
            return InnerSet.Contains(item);
        }

        bool ICollection<T>.Contains(T item)
        {
            return Contains(item);
        }

        public int IndexOf(T item)
        {
            return InnerList.IndexOf(item);
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
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            }

            return isAdded;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
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
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }

            return isRemoved;
        }

        bool ICollection<T>.Remove(T item)
        {
            return Remove(item);
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

        void ICollection<T>.Clear()
        {
            Clear();
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            // Add items starting at the last item position by default.
            AddRange(InnerSet.Count, items);
        }

        public virtual void AddRange(int startingIndex, IEnumerable<T> items)
        {
            if (startingIndex < 0 || startingIndex > InnerSet.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startingIndex));
            }

            if (items is null)
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
            if (items is null)
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
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems, 0);
            }
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

        public void CopyTo(T[] array, int arrayIndex)
        {
            InnerSet.CopyTo(array, arrayIndex);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
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

        protected virtual void ReinitializeItems()
        {
            InnerList.Clear();

            ((List<T>) InnerList).AddRange(InnerSet);

            EnsureConsistency();

            OnCollectionReset();
        }

        protected virtual void EnsureConsistency()
        {
            Contract.Assert(InnerList.Count == InnerSet.Count, "Internal data inconsistent.");
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
                    throw new InvalidOperationException($"Cannot change {nameof(ObservableSet<T>)} during a {nameof(CollectionChanged)} event.");
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
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items, startingIndex));
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
