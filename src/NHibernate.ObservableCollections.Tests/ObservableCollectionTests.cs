using System.Collections;

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
        var items = _items;

        var itemAdded = items[0];
        var itemsAddedCount = 1;
        var notificationCount = itemsAddedCount;

        var collection = new TestObservableCollection<int>();
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(0));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _itemsLarge;
        var itemsCount = items.Count;

        var itemsAdded = items;
        var itemsAddedCount = itemsCount;
        var notificationCount = itemsAddedCount;

        var collection = new TestObservableCollection<int>();
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(0));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _items;
        var itemsCount = items.Count;

        var itemRemovedIndex = 3;
        var itemRemoved = items[itemRemovedIndex];
        var itemsRemovedCount = 1;
        var notificationCount = itemsRemovedCount;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _itemsLarge;
        var itemsCount = items.Count;

        var itemsRemovedCount = itemsCount;
        var notificationCount = itemsRemovedCount;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedCount = itemsCount;
        var notificationCount = 1;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedCount = itemsCount;
        var notificationCount = 1;
        var readOnlyNotificationCount = 1;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

        Assert.That(readOnlyCollection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

        collection.Clear();

        argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        var args = argsList[0];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        Assert.That(args.OldItems, Is.Null);
        Assert.That(args.NewItems, Is.Null);

        var readOnlyArgsList = readOnlyCollection.CollectionChangedEventArgsList;

        Assert.That(readOnlyArgsList, Has.Count.EqualTo(readOnlyNotificationCount));

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
        var items = _items;
        var itemsCount = items.Count;

        var itemsAdded = items;
        var itemsAddedCount = itemsCount;
        var notificationCount = 1;

        var collection = new TestObservableCollection<int>();
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(0));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _items;
        var itemsCount = items.Count;

        var itemsAdded = items;
        var itemsAddedCount = itemsCount;
        var notificationCount = 1;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedIndex = 3;
        var itemsRemovedCount = 4;
        //var itemsRemoved = items.GetRange(itemsRemovedIndex, itemsRemovedCount);
        var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));
        var notificationCount = 1;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedIndex = 0;
        var itemsRemovedCount = itemsCount;
        //var itemsRemoved = items.GetRange(itemsRemovedIndex, itemsRemovedCount);
        var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));
        var notificationCount = 1;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedIndex = 3;
        var itemsRemovedCount = 4;
        //var itemsRemoved = items.GetRange(itemsRemovedIndex, itemsRemovedCount);
        var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));
        var notificationCount = 1;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        var items = _items;
        var itemsCount = items.Count;

        var itemsRemovedIndex = 0;
        var itemsRemovedCount = itemsCount;
        var notificationCount = 1;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;

        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

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
        // 2,6,[0,1,2,3,4,5]
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,0,1,2,3,4,5,8,9                 (10)

        var items = _items;
        var itemsCount = items.Count;

        var items_ItemsToReplace_IndexStart = 2;
        var items_ItemsToReplace_Count = 6;
        //var itemsToReplace = items.GetRange(0, 6);
        var itemsToReplace = items.GetRange(0..6);
        var itemsToReplace_Count = itemsToReplace.Count;
        var notificationCount = 1; // 1 (ReplaceRange)
        //var notificationCount = 6; // 6 (Replace)
        var itemsReplacedCount = itemsCount - items_ItemsToReplace_IndexStart;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;
        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

        collection.ReplaceRange(items_ItemsToReplace_IndexStart,
                                items_ItemsToReplace_Count,
                                itemsToReplace);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        NotifyCollectionChangedEventArgs args;
        IList? argsOldItems;
        IList? argsNewItems;

        //
        // ReplaceRange
        //
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

        //
        // Replace
        //
        //for (var i = 0; i < itemsReplacedCount - 1; i++)
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

        collectionCount = collectionCount - items_ItemsToReplace_Count + itemsToReplace_Count;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));
    }

    [Test]
    public void CanReplaceRange_WithAddRange_NonEmptyObservableCollection()
    {
        // 0,4,[0,1,2,3,4,5,6,7,8,9]
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,4,5,6,7,8,9,4,5,6,7,8,9     (16)
        // 4,4,[0,1,2,3,4,5,6,7,8,9]
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,0,1,2,3,4,5,6,7,8,9,8,9     (16)
        // 5,4,[0,1,2,3,4,5,6,7,8,9]
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,4,0,1,2,3,4,5,6,7,8,9,9     (16)
        // 6,4,[0,1,2,3,4,5,6,7,8,9]
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,4,5,0,1,2,3,4,5,6,7,8,9     (16)

        var items = _items;
        var itemsCount = items.Count;

        var items_ItemsToReplace_IndexStart = 4;
        var items_ItemsToReplace_Count = 4;
        //var itemsToReplace = items.GetRange(0, items.Count);
        var itemsToReplace = items.GetRange(0..);
        var itemsToReplace_Count = itemsToReplace.Count;
        var notificationCount = 2; // 1 (ReplaceRange) + 1 (AddRange)
        //var notificationCount = 7; // 6 (Replace) + 1 (AddRange)
        var itemsReplacedCount = itemsCount - items_ItemsToReplace_IndexStart;
        //var itemsAddedCount = itemsCount - items_ItemsToReplace_Count + itemsToReplace_Count;
        var itemsAddedCount = 6;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;
        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

        collection.ReplaceRange(items_ItemsToReplace_IndexStart,
                                items_ItemsToReplace_Count,
                                itemsToReplace);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        NotifyCollectionChangedEventArgs args;
        IList? argsOldItems;
        IList? argsNewItems;

        //
        // ReplaceRange
        //
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

        //
        // Replace
        //
        //for (var i = 0; i < itemsReplacedCount - 1; i++)
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

        collectionCount = collectionCount - items_ItemsToReplace_Count + itemsToReplace_Count;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));

        //
        // AddRange
        //
        args = argsList[notificationCount - 1];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

        argsOldItems = args.OldItems;

        Assert.That(argsOldItems, Is.Null);

        argsNewItems = args.NewItems;

        Assert.That(argsNewItems, Is.Not.Null);
        Assert.That(argsNewItems, Has.Count.EqualTo(itemsAddedCount));
    }

    [Test]
    public void CanReplaceRange_WithRemoveRange_NonEmptyObservableCollection()
    {
        // 0,6,[0,1]
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,6,7,8,9                          (6)
        // 4,6,[0,1]
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,0,1                          (6)
        // 8,4,[0,1]
        // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
        // After:   0,1,2,3,4,5,6,7,0,1                  (6)

        var items = _items;
        var itemsCount = items.Count;

        var items_ItemsToReplace_IndexStart = 4;
        var items_ItemsToReplace_Count = 6;
        //var itemsToReplace = items.GetRange(0, 2);    // 2 items
        var itemsToReplace = items.GetRange(0..2);    // 2 items
        var itemsToReplace_Count = itemsToReplace.Count;
        var notificationCount = 2; // 1 (ReplaceRange) + 1 (RemoveRange)
        //var notificationCount = 3; // 2 (Replace) + 1 (RemoveRange)
        var itemsReplacedCount = 2;
        //var itemsReplacedCount = itemsCount - items_ItemsToReplace_Count - items_ItemsToReplace_IndexStart;
        var itemsRemovedCount = 4;
        //var itemsRemovedCount = itemsCount - items_ItemsToReplace_Count + itemsToReplace_Count;

        var collection = new TestObservableCollection<int>(items);
        var collectionCount = collection.Count;
        Assert.That(collection, Has.Count.EqualTo(itemsCount));

        var argsList = collection.CollectionChangedEventArgsList;

        Assert.That(argsList, Has.Count.EqualTo(0));

        collection.ReplaceRange(items_ItemsToReplace_IndexStart,
                                items_ItemsToReplace_Count,
                                itemsToReplace);

        Assert.That(argsList, Has.Count.EqualTo(notificationCount));

        NotifyCollectionChangedEventArgs args;
        IList? argsOldItems;
        IList? argsNewItems;

        //
        // ReplaceRange
        //
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

        //
        // Replace
        //
        //for (var i = 0; i < itemsReplacedCount - 1; i++)
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

        collectionCount = collectionCount - items_ItemsToReplace_Count + itemsToReplace_Count;

        Assert.That(collection, Has.Count.EqualTo(collectionCount));

        //
        // RemoveRange
        //
        args = argsList[notificationCount - 1];

        Assert.That(args, Is.Not.Null);
        Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

        argsOldItems = args.OldItems;

        Assert.That(argsOldItems, Is.Not.Null);
        Assert.That(argsOldItems, Has.Count.EqualTo(itemsRemovedCount));

        argsNewItems = args.NewItems;

        Assert.That(argsNewItems, Is.Null);
    }
}
