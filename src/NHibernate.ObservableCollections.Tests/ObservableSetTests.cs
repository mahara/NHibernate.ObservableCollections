namespace Iesi.Collections.Generic.Tests;

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
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemAdded = items[0];
        var itemsAddedCount = 1;
        var notificationCount = itemsAddedCount;

        var collection = new ObservableSet<int>();
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(0));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.Add(itemAdded);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

        var argsNewItems = args.NewItems;

        Assert.That(argsNewItems, Is.Not.Null);
        Assert.That(argsNewItems, Has.Count.EqualTo(itemsAddedCount));
        Assert.That(args.NewStartingIndex, Is.EqualTo(-1));

        collectionCount += itemsAddedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemove_NonEmptyObservableSet()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemRemovedIndex = 3;
        var itemRemoved = items[itemRemovedIndex];
        var itemsRemovedCount = 1;
        var notificationCount = itemsRemovedCount;

        var collection = new ObservableSet<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.Remove(itemRemoved);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

        var argsOldItems = args.OldItems;

        Assert.That(argsOldItems, Is.Not.Null);
        Assert.That(argsOldItems, Has.Count.EqualTo(itemsRemovedCount));
        Assert.That(args.OldStartingIndex, Is.EqualTo(-1));

        collectionCount -= itemsRemovedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }
}
