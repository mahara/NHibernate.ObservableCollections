namespace Iesi.Collections.Generic.Tests;

using NUnit.Framework.Interfaces;

[TestFixture]
public class ObservableSetTests
{
    private readonly List<int> _items = [];

    [OneTimeSetUp]
    public void SetupFixture()
    {
        _items.Clear();
        _items.AddRange(Enumerable.Range(0, 10));
    }

    [Test]
    public void CanAdd_EmptyObservableSet()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var addedItem = items[0];
        var addedItemsCount = 1;

        var collection = new ObservableSet<int>();
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
        Assert.That(notificationCount, Is.EqualTo(1));

        var newItems = args.NewItems;

        Assert.That(newItems, Is.Not.Null);
        Assert.That(newItems, Has.Count.EqualTo(addedItemsCount));
        Assert.That(args.NewStartingIndex, Is.EqualTo(-1));

        collectionCount += addedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemove_NonEmptyObservableSet()
    {
        NotifyCollectionChangedEventArgs args = null!;
        var notificationCount = 0;

        var items = _items;
        var itemsCount = items.Count;

        var removedItemIndex = 3;
        var removedItem = items[removedItemIndex];
        var removedItemsCount = 1;

        var collection = new ObservableSet<int>(items);
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
        Assert.That(args.OldStartingIndex, Is.EqualTo(-1));

        collectionCount -= removedItemsCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }
}
