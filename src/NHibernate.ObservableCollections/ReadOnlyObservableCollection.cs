using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Iesi.Collections.Generic
{
    /// <summary>
    ///     Represents a read-only <see cref="ObservableCollection{T}" />.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the collection.
    /// </typeparam>
    /// <remarks>
    ///     AUTHORS:
    ///     -   Microsoft Corporation
    ///     -   Maximilian Haru Raditya
    ///     REFERENCES:
    ///     -   <see href="https://happynomad121.blogspot.com/2007/12/collections-for-wpf-and-nhibernate.html" />
    ///     -   <see href="https://happynomad121.blogspot.com/2008/05/revisiting-bidirectional-assoc-helpers.html" />
    ///     -   <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.readonlyobservablecollection-1" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ReadOnlyObservableCollection.cs" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ReadOnlyObservableCollection.cs" />
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
    public class ReadOnlyObservableCollection<T> :
        ReadOnlyCollection<T>,
        INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadOnlyObservableCollection{T}" /> class
        ///     that serves as a wrapper around the specified <see cref="ObservableCollection{T}" />.
        /// </summary>
        /// <param name="collection">
        ///     The <see cref="ObservableCollection{T}" /> with which to create this instance of the <see cref="ReadOnlyObservableCollection{T}" /> class.
        /// </param>
        public ReadOnlyObservableCollection(ObservableCollection<T> collection) :
            base(collection)
        {
            ((INotifyCollectionChanged) Items).CollectionChanged += OnCollectionChanged;
            ((INotifyPropertyChanged) Items).PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        ///     Occurs when an item is added, removed, or moved, or the entire collection is refreshed.
        /// </summary>
        [field: NonSerialized]
        protected virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        ///     Occurs when an item is added, removed, or moved, or the entire collection is refreshed.
        /// </summary>
        event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
        {
            add => CollectionChanged += value;
            remove => CollectionChanged -= value;
        }

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

#if !NET8_0_OR_GREATER
        /// <summary>
        ///     Gets an empty <see cref="ReadOnlyObservableCollection{T}" />.
        /// </summary>
        /// <value>An empty <see cref="ReadOnlyObservableCollection{T}" />.</value>
        /// <remarks>The returned instance is immutable and will always be empty.</remarks>
        public static ReadOnlyObservableCollection<T> Empty { get; } =
            new ReadOnlyObservableCollection<T>([]);
#endif

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event to any listeners.
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }
}
