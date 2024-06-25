namespace Iesi.Collections.Generic.Tests;

internal readonly struct CollectionChangedEventSubscription : IDisposable
{
    private readonly INotifyCollectionChanged _collection;
    private readonly NotifyCollectionChangedEventHandler _collectionChangedEventHandler;

    public CollectionChangedEventSubscription(
        INotifyCollectionChanged collection,
        NotifyCollectionChangedEventHandler collectionChangedEventHandler)
    {
        _collection = collection;
        _collectionChangedEventHandler = collectionChangedEventHandler;

        _collection.CollectionChanged += _collectionChangedEventHandler;
    }

    public void Dispose()
    {
        _collection.CollectionChanged -= _collectionChangedEventHandler;
    }
}
