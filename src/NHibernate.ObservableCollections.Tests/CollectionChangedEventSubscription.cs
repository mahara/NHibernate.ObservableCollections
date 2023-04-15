namespace Iesi.Collections.Generic.Tests
{
    internal struct CollectionChangedEventSubscription : IDisposable
    {
        private readonly INotifyCollectionChanged _collection;
        private readonly NotifyCollectionChangedEventHandler _collectionChangedMethod;

        public CollectionChangedEventSubscription(
            INotifyCollectionChanged collection,
            NotifyCollectionChangedEventHandler collectionChangedMethod)
        {
            _collection = collection;
            _collectionChangedMethod = collectionChangedMethod;

            _collection.CollectionChanged += _collectionChangedMethod;
        }

        public void Dispose()
        {
            _collection.CollectionChanged -= _collectionChangedMethod;
        }
    }
}
