namespace Iesi.Collections.Generic
{
    #region Using Directives

    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    #endregion

    /// <summary>
    ///     A read-only wrapper around <see cref="ObservableList{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    [Serializable]
    public class ReadOnlyObservableList<T> : ReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public ReadOnlyObservableList(ObservableList<T> list)
            : base(list)
        {
            ((INotifyCollectionChanged) this.InnerList).CollectionChanged += this.OnCollectionChanged;
            ((INotifyPropertyChanged) this.InnerList).PropertyChanged += this.OnPropertyChanged;
        }

        /// <summary>
        ///     CollectionChanged event (per <see cref="INotifyCollectionChanged" />).
        /// </summary>
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add => this.CollectionChanged += value;
            remove => this.CollectionChanged -= value;
        }

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        /// <inheritdoc />
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => this.PropertyChanged += value;
            remove => this.PropertyChanged -= value;
        }

        [field: NonSerialized]
        protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            this.CollectionChanged?.Invoke(this, args);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            this.PropertyChanged?.Invoke(this, args);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnCollectionChanged(e);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e);
        }
    }
}