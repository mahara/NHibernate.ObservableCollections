using System.Collections;

namespace Iesi.Collections.Generic.Tests
{
    [TestFixture]
    public class ObservableCollectionTest
    {
        private readonly List<int> _items = [];
        private readonly List<int> _itemsLarge = [];

        [OneTimeSetUp]
        public void SetupFixture()
        {
            _items.Clear();
            _items.AddRange(Enumerable.Range(0, 10));

            _itemsLarge.Clear();
            _itemsLarge.AddRange(Enumerable.Range(0, 1_000));
        }

        [Test]
        public void CanAdd_EmptyObservableCollection()
        {
            var items = _items;

            var itemAdded = items[0];
            var itemsAddedCount = 1;

            var eventsCount = itemsAddedCount;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.Add(itemAdded);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(itemsAddedCount));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(collectionCount));

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

            var eventsCount = itemsAddedCount;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            foreach (var item in itemsAdded)
            {
                collection.Add(item);
            }

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(1));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(0));

            @event = events[itemsAddedCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(1));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(itemsAddedCount - 1));

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

            var eventsCount = itemsRemovedCount;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.Remove(itemRemoved);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(itemsRemovedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(itemRemovedIndex));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanRemove_Many_NonEmptyObservableCollection()
        {
            var items = _itemsLarge;
            var itemsCount = items.Count;

            var itemsRemovedCount = itemsCount;

            var eventsCount = itemsRemovedCount;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            foreach (var item in items)
            {
                collection.Remove(item);
            }

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(1));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(0));

            @event = events[itemsRemovedCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(1));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(0));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanClear_NonEmptyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsRemovedCount = itemsCount;

            var eventsCount = 1;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.Clear();

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanClear_NonEmptyObservableCollectionInReadOnlyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsRemovedCount = itemsCount;

            var eventsCount = 1;
            var readOnlyEventsCount = 1;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

            Assert.That(readOnlyCollection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.Clear();

            events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);

            var readOnlyEvents = readOnlyCollection.CollectionChangedEventArgsList;

            Assert.That(readOnlyEvents, Has.Count.EqualTo(readOnlyEventsCount));

            var readOnlyArgs = readOnlyEvents[0];

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

            var eventsCount = 1;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.AddRange(itemsAdded);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(itemsAddedCount));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(collectionCount));

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

            var eventsCount = 1;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.AddRange(itemsAdded);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(eventsCount, Is.EqualTo(1));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(itemsAddedCount));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(itemsCount));

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

            var eventsCount = 1;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.RemoveRange(itemsRemoved);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(itemsRemovedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(itemsRemovedIndex));

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

            var eventsCount = 1;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.RemoveRange(itemsRemoved);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);

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

            var eventsCount = 1;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.RemoveRange(itemsRemoved);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(itemsRemovedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(itemsRemovedIndex));

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

            var eventsCount = 1;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);

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

            var itemsReplacedCount = itemsCount - items_ItemsToReplace_IndexStart;

            var eventsCount = 1; // 1 (ReplaceRange)
            //var eventsCount = 6; // 6 (Replace)

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.ReplaceRange(items_ItemsToReplace_IndexStart,
                                    items_ItemsToReplace_Count,
                                    itemsToReplace);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            NotifyCollectionChangedEventArgs @event;
            IList? eventOldItems;
            IList? eventNewItems;

            //
            // ReplaceRange
            //
            @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(itemsReplacedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart));

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(itemsReplacedCount));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart));

            //
            // Replace
            //
            //for (var i = 0; i < itemsReplacedCount - 1; i++)
            //{
            //    @event = events[i];
            //
            //    Assert.That(@event, Is.Not.Null);
            //    Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
            //
            //    eventOldItems = @event.OldItems;
            //
            //    Assert.That(eventOldItems, Is.Not.Null);
            //    Assert.That(eventOldItems, Has.Count.EqualTo(1));
            //    Assert.That(@event.OldStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart + i));
            //
            //    eventNewItems = @event.NewItems;
            //
            //    Assert.That(eventNewItems, Is.Not.Null);
            //    Assert.That(eventNewItems, Has.Count.EqualTo(1));
            //    Assert.That(@event.NewStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart + i));
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

            var itemsReplacedCount = itemsCount - items_ItemsToReplace_IndexStart;
            //var itemsAddedCount = itemsCount - items_ItemsToReplace_Count + itemsToReplace_Count;
            var itemsAddedCount = 6;

            var eventsCount = 2; // 1 (ReplaceRange) + 1 (AddRange)
            //var eventsCount = 7; // 6 (Replace) + 1 (AddRange)

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.ReplaceRange(items_ItemsToReplace_IndexStart,
                                    items_ItemsToReplace_Count,
                                    itemsToReplace);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            NotifyCollectionChangedEventArgs @event;
            IList? eventOldItems;
            IList? eventNewItems;

            //
            // ReplaceRange
            //
            @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(itemsReplacedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart));

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(itemsReplacedCount));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart));

            //
            // Replace
            //
            //for (var i = 0; i < itemsReplacedCount - 1; i++)
            //{
            //    @event = events[i];
            //
            //    Assert.That(@event, Is.Not.Null);
            //    Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
            //
            //    eventOldItems = @event.OldItems;
            //
            //    Assert.That(eventOldItems, Is.Not.Null);
            //    Assert.That(eventOldItems, Has.Count.EqualTo(1));
            //    Assert.That(@event.OldStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart + i));
            //
            //    eventNewItems = @event.NewItems;
            //
            //    Assert.That(eventNewItems, Is.Not.Null);
            //    Assert.That(eventNewItems, Has.Count.EqualTo(1));
            //    Assert.That(@event.NewStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart + i));
            //}

            collectionCount = collectionCount - items_ItemsToReplace_Count + itemsToReplace_Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            //
            // AddRange
            //
            @event = events[eventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(itemsAddedCount));
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

            var itemsReplacedCount = 2;
            //var itemsReplacedCount = itemsCount - items_ItemsToReplace_Count - items_ItemsToReplace_IndexStart;
            var itemsRemovedCount = 4;
            //var itemsRemovedCount = itemsCount - items_ItemsToReplace_Count + itemsToReplace_Count;

            var eventsCount = 2; // 1 (ReplaceRange) + 1 (RemoveRange)
            //var eventsCount = 3; // 2 (Replace) + 1 (RemoveRange)

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var events = collection.CollectionChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));

            collection.ReplaceRange(items_ItemsToReplace_IndexStart,
                                    items_ItemsToReplace_Count,
                                    itemsToReplace);

            Assert.That(events, Has.Count.EqualTo(eventsCount));

            NotifyCollectionChangedEventArgs @event;
            IList? eventOldItems;
            IList? eventNewItems;

            //
            // ReplaceRange
            //
            @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(itemsReplacedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart));

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(itemsReplacedCount));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart));

            //
            // Replace
            //
            //for (var i = 0; i < itemsReplacedCount - 1; i++)
            //{
            //    @event = events[i];
            //
            //    Assert.That(@event, Is.Not.Null);
            //    Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
            //
            //    eventOldItems = @event.OldItems;
            //
            //    Assert.That(eventOldItems, Is.Not.Null);
            //    Assert.That(eventOldItems, Has.Count.EqualTo(1));
            //    Assert.That(@event.OldStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart + i));
            //
            //    eventNewItems = @event.NewItems;
            //
            //    Assert.That(eventNewItems, Is.Not.Null);
            //    Assert.That(eventNewItems, Has.Count.EqualTo(1));
            //    Assert.That(@event.NewStartingIndex, Is.EqualTo(items_ItemsToReplace_IndexStart + i));
            //}

            collectionCount = collectionCount - items_ItemsToReplace_Count + itemsToReplace_Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            //
            // RemoveRange
            //
            @event = events[eventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(itemsRemovedCount));

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }
    }
}
