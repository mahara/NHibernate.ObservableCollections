namespace Iesi.Collections.Generic.Tests;

[TestFixture]
public class ObservableCollectionTests
{
    private readonly List<int> _items = [];
    private readonly List<int> _itemsLarge = [];

    [OneTimeSetUp]
    public void SetupFixture()
    {
        _items.Clear();
        _items.AddRange(Enumerable.Range(0, 10));

        _itemsLarge.Clear();
        _itemsLarge.AddRange(Enumerable.Range(0, 100_000));
    }

    [Test]
    public void CanAdd_EmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var addedItem = items[0];
        var addedItemsCount = 1;

        var collection = new ObservableCollection<int>();
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(0));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.Add(addedItem);

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
        Assert.That(notificationCount, Is.EqualTo(addedItemsCount));

        var newItems = args.NewItems;

        Assert.That(newItems, Is.Not.Null);
        Assert.That(newItems, Has.Count.EqualTo(addedItemsCount));
        Assert.That(args.NewStartingIndex, Is.EqualTo(collectionCount));

        collectionCount += addedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanAdd_Many_EmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _itemsLarge;
        var itemsCount = items.Count;

        var addedItems = items;
        var addedItemsCount = itemsCount;

        var collection = new ObservableCollection<int>();
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(0));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        foreach (var item in addedItems)
        {
            collection.Add(item);
        }

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
        Assert.That(notificationCount, Is.EqualTo(addedItemsCount));

        var newItems = args.NewItems;

        Assert.That(newItems, Is.Not.Null);
        Assert.That(newItems, Has.Count.EqualTo(1));
        Assert.That(args.NewStartingIndex, Is.EqualTo(addedItemsCount - 1));

        collectionCount += addedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemove_NonEmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var removedItemIndex = 3;
        var removedItem = items[removedItemIndex];
        var removedItemsCount = 1;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.Remove(removedItem);

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
        Assert.That(notificationCount, Is.EqualTo(1));

        var oldItems = args.OldItems;

        Assert.That(oldItems, Is.Not.Null);
        Assert.That(oldItems, Has.Count.EqualTo(removedItemsCount));
        Assert.That(args.OldStartingIndex, Is.EqualTo(removedItemIndex));

        collectionCount -= removedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemove_Many_NonEmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _itemsLarge;
        var itemsCount = items.Count;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        foreach (var item in items)
        {
            collection.Remove(item);
        }

        var removedItemsCount = itemsCount;

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
        Assert.That(notificationCount, Is.EqualTo(removedItemsCount));

        var oldItems = args.OldItems;

        Assert.That(oldItems, Is.Not.Null);
        Assert.That(oldItems, Has.Count.EqualTo(1));
        Assert.That(args.OldStartingIndex, Is.EqualTo(0));

        collectionCount -= removedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanClear_NonEmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.Clear();
        var removedItemsCount = itemsCount;

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(args.OldItems, Is.Null);
        Assert.That(args.NewItems, Is.Null);
        Assert.That(notificationCount, Is.EqualTo(1));

        collectionCount -= removedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanClear_NonEmptyObservableCollectionInReadOnlyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        NotifyCollectionChangedEventArgs readOnlyArgs = null!;
        var readOnlyNotificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var readOnlyCollection = new ReadOnlyObservableCollection<int>(collection);

        Assert.That(readOnlyCollection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        using var __ = new CollectionChangedEventSubscription(
            readOnlyCollection,
            (o, e) =>
            {
                readOnlyArgs = e;
                readOnlyNotificationCount++;
            });


        collection.Clear();
        var removedItemsCount = itemsCount;

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(args.OldItems, Is.Null);
        Assert.That(args.NewItems, Is.Null);
        Assert.That(notificationCount, Is.EqualTo(1));

        Assert.That(readOnlyArgs, Is.Not.Null);
        Assert.That(readOnlyArgs.OldItems, Is.Null);
        Assert.That(readOnlyArgs.NewItems, Is.Null);
        Assert.That(readOnlyNotificationCount, Is.EqualTo(1));

        collectionCount -= removedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
        Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanAddRange_EmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var addedItems = items;
        var addedItemsCount = itemsCount;

        var collection = new ObservableCollection<int>();
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(0));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.AddRange(addedItems);

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
        Assert.That(notificationCount, Is.EqualTo(1));

        var newItems = args.NewItems;

        Assert.That(newItems, Is.Not.Null);
        Assert.That(newItems, Has.Count.EqualTo(addedItemsCount));
        Assert.That(args.NewStartingIndex, Is.EqualTo(collectionCount));

        collectionCount += addedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanAddRange_NonEmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var addedItems = items;
        var addedItemsCount = itemsCount;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.AddRange(addedItems);

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
        Assert.That(notificationCount, Is.EqualTo(1));

        var newItems = args.NewItems;

        Assert.That(newItems, Is.Not.Null);
        Assert.That(newItems, Has.Count.EqualTo(addedItemsCount));
        Assert.That(args.NewStartingIndex, Is.EqualTo(itemsCount));

        collectionCount += addedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemoveRange_NonEmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var removedItemsIndex = 3;
        var removedItemsCount = 4;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.RemoveRange(items.GetRange(removedItemsIndex, removedItemsCount));

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
        Assert.That(notificationCount, Is.EqualTo(1));

        var oldItems = args.OldItems;

        Assert.That(oldItems, Is.Not.Null);
        Assert.That(oldItems, Has.Count.EqualTo(removedItemsCount));
        Assert.That(args.OldStartingIndex, Is.GreaterThanOrEqualTo(0));

        collectionCount -= removedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemoveRangeAll_NonEmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var removedItemsIndex = 0;
        var removedItemsCount = itemsCount;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.RemoveRange(items.GetRange(removedItemsIndex, removedItemsCount));

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(notificationCount, Is.EqualTo(1));

        collectionCount -= removedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemoveRangeByIndexAndCount_NonEmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var removedItemsIndex = 3;
        var removedItemsCount = 4;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.RemoveRange(removedItemsIndex, removedItemsCount);

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
        Assert.That(notificationCount, Is.EqualTo(1));

        var oldItems = args.OldItems;

        Assert.That(oldItems, Is.Not.Null);
        Assert.That(oldItems, Has.Count.EqualTo(removedItemsCount));
        Assert.That(args.OldStartingIndex, Is.EqualTo(removedItemsIndex));

        collectionCount -= removedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemoveRangeAllByIndexAndCount_NonEmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var removedItemsIndex = 0;
        var removedItemsCount = itemsCount;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.RemoveRange(removedItemsIndex, removedItemsCount);

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(notificationCount, Is.EqualTo(1));

        collectionCount -= removedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanReplaceRange_NonEmptyObservableCollection()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var replaceStartingIndex = 4;
        var replacedItemsCount = 4;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) =>
            {
                args = e;
                notificationCount++;
            });

        collection.ReplaceRange(replaceStartingIndex, replacedItemsCount, items);

        Assert.That(args, Is.Not.Null);
        //
        // TODO:    Should change ReplaceRangeCore implementation to an optimal one,
        //          without first removing and then adding.
        //
        //Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
        Assert.That(notificationCount, Is.GreaterThanOrEqualTo(1));

        var newItems = args.NewItems;

        Assert.That(newItems, Is.Not.Null);
        Assert.That(newItems, Has.Count.EqualTo(itemsCount));
        Assert.That(args.NewStartingIndex, Is.EqualTo(replaceStartingIndex));

        collectionCount = collectionCount - replacedItemsCount + itemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }
}
