namespace Iesi.Collections.Generic.Tests;

using System.Collections;

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
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemAdded = items[0];
        var itemsAddedCount = 1;
        var notificationCount = itemsAddedCount;

        var collection = new ObservableCollection<int>();
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
        Assert.That(args.NewStartingIndex, Is.EqualTo(collectionCount));

        collectionCount += itemsAddedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanAdd_Many_EmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _itemsLarge;
        var itemsCount = items.Count;

        var itemsAdded = items;
        var itemsAddedCount = itemsCount;
        var notificationCount = itemsAddedCount;

        var collection = new ObservableCollection<int>();
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(0));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        foreach (var item in itemsAdded)
        {
            collection.Add(item);
        }

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

        var argsNewItems = args.NewItems;

        Assert.That(argsNewItems, Is.Not.Null);
        Assert.That(argsNewItems, Has.Count.EqualTo(1));
        Assert.That(args.NewStartingIndex, Is.EqualTo(0));

        args = argsList[itemsAddedCount - 1];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

        argsNewItems = args.NewItems;

        Assert.That(argsNewItems, Is.Not.Null);
        Assert.That(argsNewItems, Has.Count.EqualTo(1));
        Assert.That(args.NewStartingIndex, Is.EqualTo(itemsAddedCount - 1));

        collectionCount += itemsAddedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemove_NonEmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemRemovedIndex = 3;
        var itemRemoved = items[itemRemovedIndex];
        var itemsRemovedCount = 1;
        var notificationCount = itemsRemovedCount;

        var collection = new ObservableCollection<int>(items);
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
        Assert.That(args.OldStartingIndex, Is.EqualTo(itemRemovedIndex));

        collectionCount -= itemsRemovedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemove_Many_NonEmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _itemsLarge;
        var itemsCount = items.Count;

        var itemsRemovedCount = itemsCount;
        var notificationCount = itemsRemovedCount;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        foreach (var item in items)
        {
            collection.Remove(item);
        }

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

        var argsOldItems = args.OldItems;

        Assert.That(argsOldItems, Is.Not.Null);
        Assert.That(argsOldItems, Has.Count.EqualTo(1));
        Assert.That(args.OldStartingIndex, Is.EqualTo(0));

        args = argsList[itemsRemovedCount - 1];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

        argsOldItems = args.OldItems;

        Assert.That(argsOldItems, Is.Not.Null);
        Assert.That(argsOldItems, Has.Count.EqualTo(1));
        Assert.That(args.OldStartingIndex, Is.EqualTo(0));

        collectionCount -= itemsRemovedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanClear_NonEmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedCount = itemsCount;
        var notificationCount = 1;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.Clear();

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(args.OldItems, Is.Null);
        Assert.That(args.NewItems, Is.Null);

        collectionCount -= itemsRemovedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanClear_NonEmptyObservableCollectionInReadOnlyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();
        var readOnlyArgsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedCount = itemsCount;
        var notificationCount = 1;
        var readOnlyNotificationCount = 1;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var readOnlyCollection = new ReadOnlyObservableCollection<int>(collection);

        Assert.That(readOnlyCollection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        using var __ = new CollectionChangedEventSubscription(
            readOnlyCollection,
            (o, e) => readOnlyArgsList.Add(e));

        collection.Clear();

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));
        Assert.That(readOnlyArgsList, Has.Count.EqualTo(readOnlyNotificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(args.OldItems, Is.Null);
        Assert.That(args.NewItems, Is.Null);

        var readOnlyArgs = readOnlyArgsList[0];

        Assert.That(readOnlyArgs, Is.Not.Null);
        Assert.That(readOnlyArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(readOnlyArgs.OldItems, Is.Null);
        Assert.That(readOnlyArgs.NewItems, Is.Null);

        collectionCount -= itemsRemovedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
        Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanAddRange_EmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemsAdded = items;
        var itemsAddedCount = itemsCount;
        var notificationCount = 1;

        var collection = new ObservableCollection<int>();
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(0));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.AddRange(itemsAdded);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

        var argsNewItems = args.NewItems;

        Assert.That(argsNewItems, Is.Not.Null);
        Assert.That(argsNewItems, Has.Count.EqualTo(itemsAddedCount));
        Assert.That(args.NewStartingIndex, Is.EqualTo(collectionCount));

        collectionCount += itemsAddedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanAddRange_NonEmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemsAdded = items;
        var itemsAddedCount = itemsCount;
        var notificationCount = 1;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.AddRange(itemsAdded);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
        Assert.That(notificationCount, Is.EqualTo(1));

        var argsNewItems = args.NewItems;

        Assert.That(argsNewItems, Is.Not.Null);
        Assert.That(argsNewItems, Has.Count.EqualTo(itemsAddedCount));
        Assert.That(args.NewStartingIndex, Is.EqualTo(itemsCount));

        collectionCount += itemsAddedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemoveRange_NonEmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedIndex = 3;
        var itemsRemovedCount = 4;
        var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));
        //var itemsRemoved = items.GetRange(itemsRemovedIndex, itemsRemovedCount);
        var notificationCount = 1;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.RemoveRange(itemsRemoved);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

        var argsOldItems = args.OldItems;

        Assert.That(argsOldItems, Is.Not.Null);
        Assert.That(argsOldItems, Has.Count.EqualTo(itemsRemovedCount));
        Assert.That(args.OldStartingIndex, Is.EqualTo(itemsRemovedIndex));

        collectionCount -= itemsRemovedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemoveRangeAll_NonEmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedIndex = 0;
        var itemsRemovedCount = itemsCount;
        var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));
        //var itemsRemoved = items.GetRange(itemsRemovedIndex, itemsRemovedCount);
        var notificationCount = 1;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.RemoveRange(itemsRemoved);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(args.OldItems, Is.Null);
        Assert.That(args.NewItems, Is.Null);

        collectionCount -= itemsRemovedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemoveRangeByIndexAndCount_NonEmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedIndex = 3;
        var itemsRemovedCount = 4;
        var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));
        //var itemsRemoved = items.GetRange(itemsRemovedIndex, itemsRemovedCount);
        var notificationCount = 1;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.RemoveRange(itemsRemoved);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

        var argsOldItems = args.OldItems;

        Assert.That(argsOldItems, Is.Not.Null);
        Assert.That(argsOldItems, Has.Count.EqualTo(itemsRemovedCount));
        Assert.That(args.OldStartingIndex, Is.EqualTo(itemsRemovedIndex));

        collectionCount -= itemsRemovedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanRemoveRangeAllByIndexAndCount_NonEmptyObservableCollection()
    {
        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedIndex = 0;
        var itemsRemovedCount = itemsCount;
        var notificationCount = 1;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(args.OldItems, Is.Null);
        Assert.That(args.NewItems, Is.Null);

        collectionCount -= itemsRemovedCount;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanReplaceRange_NonEmptyObservableCollection()
    {
        // 0,4,items
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,4,5,6,7,8,9,4,5,6,7,8,9     (16)
        // 4,4,items
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,0,1,2,3,4,5,6,7,8,9,8,9     (16)
        // 5,4,items
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,4,0,1,2,3,4,5,6,7,8,9,9     (16)
        // 6,4,items
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,4,5,0,1,2,3,4,5,6,7,8,9     (16)

        var argsList = new List<NotifyCollectionChangedEventArgs>();

        var items = _items;
        var itemsCount = items.Count;

        var items_ItemsToReplace_IndexStart = 4;
        var items_ItemsToReplace_Count = 4;
        var items_ItemsToReplace = items;
        var notificationCount = 2; // 1 (ReplaceRange) + 1 (AddRange)
        //var notificationCount = 7; // 6 (Replace) + 1 (AddRange)
        var itemsReplacedCount = itemsCount - items_ItemsToReplace_IndexStart;
        //var itemsAddedCount = itemsCount + itemsCount - items_ItemsToReplace_Count;

        var collection = new ObservableCollection<int>(items);
        var collectionCount = collection.Count;
        var collectionCountOld = collectionCount;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        using var _ = new CollectionChangedEventSubscription(
            collection,
            (o, e) => argsList.Add(e));

        collection.ReplaceRange(items_ItemsToReplace_IndexStart,
                                items_ItemsToReplace_Count,
                                items_ItemsToReplace);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        NotifyCollectionChangedEventArgs args;
        IList? argsOldItems;
        IList? argsNewItems;

        // ReplaceRange
        args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

        argsOldItems = args.OldItems;

        Assert.That(argsOldItems, Is.Not.Null);
        Assert.That(argsOldItems, Has.Count.EqualTo(itemsReplacedCount));
        Assert.That(args.OldStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart));

        argsNewItems = args.NewItems;

        Assert.That(argsNewItems, Is.Not.Null);
        Assert.That(argsNewItems, Has.Count.EqualTo(itemsReplacedCount));
        Assert.That(args.NewStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart));

        //// Replace
        //for (var i = 0; i < notificationCount - 1; i++)
        //{
        //    args = argsList[i];

        //    Assert.That(args, Is.Not.Null);
        //    Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

        //    argsOldItems = args.OldItems;

        //    Assert.That(argsOldItems, Is.Not.Null);
        //    Assert.That(argsOldItems, Has.Count.EqualTo(1));
        //    Assert.That(args.OldStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart + i));

        //    argsNewItems = args.NewItems;

        //    Assert.That(argsNewItems, Is.Not.Null);
        //    Assert.That(argsNewItems, Has.Count.EqualTo(1));
        //    Assert.That(args.NewStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart + i));
        //}

        // AddRange
        args = argsList[notificationCount - 1];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

        argsOldItems = args.OldItems;

        Assert.That(argsOldItems, Is.Null);

        collectionCount = collectionCount - items_ItemsToReplace_Count + itemsCount;
        argsNewItems = args.NewItems;

        Assert.That(argsNewItems, Is.Not.Null);
        Assert.That(argsNewItems, Has.Count.EqualTo(collection.Count - collectionCountOld));
        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }
}
