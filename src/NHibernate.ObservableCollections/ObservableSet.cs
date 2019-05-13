namespace Iesi.Collections.Generic
{
    #region Using Directives

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Runtime.InteropServices;

    #endregion

    /// <summary>
    ///     A generic set that fires events when item(s)
    ///     have been added to or removed from the set.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the set.
    /// </typeparam>
    /// <author>Adrian Alexander</author>
    /// <author>Maximilian Haru Raditya</author>
    /// <author>Microsoft Corporation</author>
    /// <remarks>
    ///     <see href="http://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs" />
    /// </remarks>
    [Serializable]
    [ComVisible(false)]
    [DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
    // ReSharper disable once UseNameofExpression
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableSet<T> : ISet<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        protected const string CountPropertyName = "Count";

        private readonly SimpleMonitor _monitor = new SimpleMonitor();

        public ObservableSet()
        {
        }

        public ObservableSet(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            this.Initialize(collection);
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
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        ///     PropertyChanged event (per <see cref="T:System.ComponentModel.INotifyPropertyChanged" />).
        /// </summary>
        /// <inheritdoc />
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => this.PropertyChanged += value;
            remove => this.PropertyChanged -= value;
        }

        public int Count =>
            this.InnerSet.Count;

        public bool IsReadOnly =>
            this.InnerSet.IsReadOnly;

        public IEnumerator<T> GetEnumerator()
        {
            return this.InnerSet.GetEnumerator();
        }

        public virtual bool Add(T item)
        {
            this.CheckReentrancy();

            this.EnsureConsistency();

            var isAdded = this.InnerSet.Add(item);
            if (isAdded)
            {
                this.InnerList.Add(item);

                this.EnsureConsistency();

                var index = this.InnerSet.Count - 1;

                this.OnPropertyChanged(CountPropertyName);
                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            }

            return isAdded;
        }

        public virtual void UnionWith(IEnumerable<T> other)
        {
            this.InnerSet.UnionWith(other);

            this.ReinitializeItems();
        }

        public virtual void IntersectWith(IEnumerable<T> other)
        {
            this.InnerSet.IntersectWith(other);

            this.ReinitializeItems();
        }

        public virtual void ExceptWith(IEnumerable<T> other)
        {
            this.InnerSet.ExceptWith(other);

            this.ReinitializeItems();
        }

        public virtual void SymmetricExceptWith(IEnumerable<T> other)
        {
            this.InnerSet.SymmetricExceptWith(other);

            this.ReinitializeItems();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return this.InnerSet.IsSubsetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return this.InnerSet.IsProperSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return this.InnerSet.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return this.InnerSet.IsProperSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return this.InnerSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return this.InnerSet.SetEquals(other);
        }

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        bool ICollection<T>.Remove(T item)
        {
            return this.Remove(item);
        }

        void ICollection<T>.Clear()
        {
            this.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return this.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) this.InnerSet).GetEnumerator();
        }

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual void AddRange(IEnumerable<T> items)
        {
            // Add items starting at the last item position by default.
            var startingIndex = this.InnerSet.Count;

            this.AddRange(startingIndex, items);
        }

        public virtual void AddRange(int startingIndex, IEnumerable<T> items)
        {
            if (startingIndex < 0 || startingIndex > this.InnerSet.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startingIndex));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.CheckReentrancy();

            this.EnsureConsistency();

            var addedItems = new HashSet<T>();
            foreach (var item in items)
            {
                if (this.InnerSet.Add(item))
                {
                    this.InnerList.Add(item);

                    this.EnsureConsistency();

                    addedItems.Add(item);
                }
            }

            this.EnsureConsistency();

            if (addedItems.Count > 0)
            {
                this.OnPropertyChanged(CountPropertyName);
                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, addedItems, startingIndex);
            }
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.CheckReentrancy();

            this.EnsureConsistency();

            var removedItems = new List<T>();
            foreach (var item in items)
            {
                if (this.InnerSet.Remove(item))
                {
                    this.InnerList.Remove(item);

                    this.EnsureConsistency();

                    removedItems.Add(item);
                }
            }

            this.EnsureConsistency();

            if (removedItems.Count > 0)
            {
                this.OnPropertyChanged(CountPropertyName);
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems.ToArray(), 0);
            }
        }

        public virtual bool Remove(T item)
        {
            this.CheckReentrancy();

            this.EnsureConsistency();

            var index = this.InnerList.IndexOf(item);
            var isRemoved = index >= 0 && this.InnerSet.Remove(item);
            if (isRemoved)
            {
                this.InnerList.RemoveAt(index);

                this.EnsureConsistency();

                this.OnPropertyChanged(CountPropertyName);
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }

            return isRemoved;
        }

        public virtual void Clear()
        {
            this.CheckReentrancy();

            this.EnsureConsistency();

            this.InnerSet.Clear();
            this.InnerList.Clear();

            this.EnsureConsistency();

            this.OnPropertyChanged(CountPropertyName);
            this.OnCollectionReset();
        }

        public bool Contains(T item)
        {
            return this.InnerSet.Contains(item);
        }

        public int IndexOf(T item)
        {
            return this.InnerList.IndexOf(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.InnerSet.CopyTo(array, arrayIndex);
        }

        private void Initialize(IEnumerable<T> collection)
        {
            this.EnsureConsistency();

            foreach (var item in collection)
            {
                if (this.InnerSet.Add(item))
                {
                    this.InnerList.Add(item);

                    this.EnsureConsistency();
                }
            }
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
                    throw new InvalidOperationException("Cannot change ObservableSet during a CollectionChanged event.");
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

        protected virtual void EnsureConsistency()
        {
            Contract.Assert(this.InnerList.Count == this.InnerSet.Count, "Internal data inconsistent.");
        }

        protected virtual void ReinitializeItems()
        {
            this.InnerList.Clear();
            ((List<T>) this.InnerList).AddRange(this.InnerSet);

            this.EnsureConsistency();

            this.OnCollectionReset();
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int startingIndex)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, startingIndex));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<T> items, int startingIndex)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, (IList) items, startingIndex));
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

        protected void OnCollectionReset()
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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