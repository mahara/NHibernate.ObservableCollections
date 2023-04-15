namespace Iesi.Collections.Generic
{
    using System.Diagnostics;

    /// <summary>
    ///     A read-only wrapper around <see cref="ObservableList{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    /// <remarks>
    ///     REFERENCES:
    ///     -   https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ReadOnlyObservableCollection.cs
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
    public class ReadOnlyObservableList<T> :
        ReadOnlyList<T>,
        INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        ///     Initializes a new instance of ReadOnlyObservableList
        ///     that wraps the given ObservableCollection.
        /// </summary>
        public ReadOnlyObservableList(ObservableList<T> list) : base(list)
        {
            ((INotifyCollectionChanged) Items).CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
            ((INotifyPropertyChanged) Items).PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
        }

        /// <summary>
        ///     Gets an empty <see cref="ReadOnlyObservableList{T}" />.
        /// </summary>
        /// <value>An empty <see cref="ReadOnlyObservableList{T}" />.</value>
        /// <remarks>The returned instance is immutable and will always be empty.</remarks>
        public static ReadOnlyObservableList<T> Empty { get; } =
            new ReadOnlyObservableList<T>(new ObservableList<T>());

        /// <summary>
        ///     CollectionChanged event (per <see cref="INotifyCollectionChanged" />).
        /// </summary>
        event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
        {
            add => CollectionChanged += value;
            remove => CollectionChanged -= value;
        }

        /// <summary>
        ///     Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        /// <remarks>
        ///     See <seealso cref="INotifyCollectionChanged" />.
        /// </remarks>
        [field: NonSerialized]
        protected virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        ///     Raise CollectionChanged event to any listeners.
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
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
        ///     Occurs when a property changes.
        /// </summary>
        /// <remarks>
        ///     See <seealso cref="INotifyPropertyChanged" />.
        /// </remarks>
        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Raise PropertyChanged event to any listeners.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }
    }
}
