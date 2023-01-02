namespace Iesi.Collections.Generic.Tests
{
    internal readonly struct CollectionChangedEventSubscription : IDisposable
    {
        private readonly INotifyCollectionChanged _collection;
        private readonly NotifyCollectionChangedEventHandler _onCollectionChanged;

        public CollectionChangedEventSubscription(
            INotifyCollectionChanged collection,
            NotifyCollectionChangedEventHandler onCollectionChanged)
        {
            _collection = collection;
            _onCollectionChanged = onCollectionChanged;

            _collection.CollectionChanged += _onCollectionChanged;
        }

        public void Dispose()
        {
            _collection.CollectionChanged -= _onCollectionChanged;
        }
    }
}
