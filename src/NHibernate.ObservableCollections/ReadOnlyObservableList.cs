namespace Iesi.Collections.Generic
{
    using System.ComponentModel;

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
            ((INotifyCollectionChanged) InnerList).CollectionChanged += OnCollectionChanged;
            ((INotifyPropertyChanged) InnerList).PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        ///     CollectionChanged event (per <see cref="INotifyCollectionChanged" />).
        /// </summary>
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add => CollectionChanged += value;
            remove => CollectionChanged -= value;
        }

        /// <summary>
        ///     PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        /// <inheritdoc />
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        [field: NonSerialized]
        protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }
    }
}
