using System.Collections;

namespace Iesi.Collections.Generic.Tests
{
    [TestFixture(EventNotificationOrderMode.Strict)]
    [TestFixture(EventNotificationOrderMode.Relaxed)]
    public class ObservableCollectionTests : TestBase
    {
        private readonly List<int> _items = [];
        private readonly List<int> _itemsLarge = [];

        #region Setup

        public ObservableCollectionTests(EventNotificationOrderMode eventNotificationOrderMode) :
            base(eventNotificationOrderMode)
        {
        }

        [OneTimeSetUp]
        public void SetupFixture()
        {
            _items.Clear();
            _items.AddRange(Enumerable.Range(0, 10));

            _itemsLarge.Clear();
            _itemsLarge.AddRange(Enumerable.Range(0, 1_000));
        }

        #endregion



        #region Refresh

        [Test]
        public void CanRefresh()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];

            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Refresh
            //
            collection.Refresh();

            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        #endregion



        #region SetItem

        [Test]
        public void CanSetItem_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemSetIndex = 3;
            var itemOld = items[itemSetIndex];
            var itemNew = 100;

            var expectedItems = items.Select((item, index) => index == itemSetIndex ? itemNew : item)
                                     .ToArray();

            int[] expectedReplace_Event_OldItems = [itemOld];
            var expectedReplace_Event_OldStartingIndex = itemSetIndex;
            int[] expectedReplace_Event_NewItems = [itemNew];
            var expectedReplace_Event_NewStartingIndex = itemSetIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // SetItem
            //
            collection[itemSetIndex] = itemNew;

            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedReplace_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedReplace_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedReplace_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedReplace_Event_NewStartingIndex));
        }

        #endregion



        #region Add / Insert

        [Test]
        public void CanAdd_EmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAdded = items[0];
            var itemsAddedCount = 1;

            int[] expectedItems = [itemAdded];

            int[] expectedAdd_Event_NewItems = [itemAdded];
            var expectedAdd_Event_NewItemsCount = expectedAdd_Event_NewItems.Length;
            var expectedAdd_Event_NewStartingIndex = 0;
            var expectedEventCount = itemsAddedCount;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Add
            //
            collection.Add(itemAdded);

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedAdd_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedAdd_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedAdd_Event_NewStartingIndex));
        }

        [Test]
        public void CanAdd_Multiple_EmptyObservableCollection()
        {
            var items = _itemsLarge;
            //var itemsCount = items.Count;

            int[] itemsAdded = [.. items];
            var itemsAddedCount = itemsAdded.Length;

            int[] expectedItems = [.. itemsAdded];

            int[] expectedAdd_Event_NewItems_Item_First = [itemsAdded[0]];
            var expectedAdd_Event_NewItemsCount = expectedAdd_Event_NewItems_Item_First.Length;
            var expectedAdd_Event_NewStartingIndex_Item_First = 0;
            int[] expectedAdd_Event_NewItems_Item_Last = [itemsAdded[itemsAddedCount - 1]];
            var expectedAdd_Event_NewStartingIndex_Item_Last = itemsAddedCount - 1;
            var expectedEventCount = itemsAddedCount;
            var expectedPropertyChangedEventPatternCount = expectedEventCount;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Add (Multiple)
            //
            foreach (var item in itemsAdded)
            {
                collection.Add(item);
            }

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPatternCount, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedAdd_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedAdd_Event_NewItems_Item_First));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedAdd_Event_NewStartingIndex_Item_First));

            @event = events[itemsAddedCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedAdd_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedAdd_Event_NewItems_Item_Last));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedAdd_Event_NewStartingIndex_Item_Last));
        }



        [Test]
        public void CanInsert_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = 3;
            var itemInserted = 100;
            var itemsInsertedCount = 1;

            var expectedItems = items.Take(itemInsertedIndex)
                                     .Concat([itemInserted])
                                     .Concat(items.Skip(itemInsertedIndex))
                                     .ToArray();

            int[] expectedAdd_Event_NewItems = [itemInserted];
            var expectedAdd_Event_NewStartingIndex = itemInsertedIndex;
            var expectedEventCount = itemsInsertedCount;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Insert
            //
            collection.Insert(itemInsertedIndex, itemInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedAdd_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedAdd_Event_NewStartingIndex));
        }

        #endregion



        #region Remove / RemoveAt

        [Test]
        public void CanRemove_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            int[] expectedRemove_Event_OldItems = [itemRemoved];
            var expectedRemove_Event_OldItemsCount = expectedRemove_Event_OldItems.Length;
            var expectedRemove_Event_OldStartingIndex = itemRemovedIndex;
            var expectedEventCount = itemsRemovedCount;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Remove
            //
            collection.Remove(itemRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemove_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanRemove_Multiple_NonEmptyObservableCollection()
        {
            var items = _itemsLarge;
            //var itemsCount = items.Count;

            int[] itemsRemoved = [.. items];
            var itemsRemovedCount = itemsRemoved.Length;

            //int[] expectedItems = [];

            int[] expectedRemove_Event_OldItems_Item_First = [itemsRemoved[0]];
            var expectedRemove_Event_OldItemsCount = expectedRemove_Event_OldItems_Item_First.Length;
            var expectedRemove_Event_OldStartingIndex_Item_First = 0;
            int[] expectedRemove_Event_OldItems_Item_Last = [itemsRemoved[itemsRemovedCount - 1]];
            var expectedRemove_Event_OldStartingIndex_Item_Last = 0;
            var expectedEventCount = itemsRemovedCount;
            var expectedPropertyChangedEventPatternCount = expectedEventCount;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Remove (Multiple)
            //
            foreach (var item in items)
            {
                collection.Remove(item);
            }

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPatternCount, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemove_Event_OldItems_Item_First));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemove_Event_OldStartingIndex_Item_First));

            @event = events[itemsRemovedCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemove_Event_OldItems_Item_Last));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemove_Event_OldStartingIndex_Item_Last));
        }



        [Test]
        public void CanRemoveAt_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Take(itemRemovedIndex)
                                     .Concat(items.Skip(itemRemovedIndex + itemsRemovedCount))
                                     .ToArray();

            int[] expectedRemove_Event_OldItems = [itemRemoved];
            var expectedRemove_Event_OldStartingIndex = itemRemovedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveAt
            //
            collection.RemoveAt(itemRemovedIndex);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemove_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        #endregion



        #region Move

        [Test]
        public void CanMove_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemIndexOld = 3;
            var itemIndexNew = 6;
            var itemMoved = items[itemIndexOld];

            var expectedItemsList = items.ToList();
            expectedItemsList.RemoveAt(itemIndexOld);
            expectedItemsList.Insert(itemIndexNew, itemMoved);
            int[] expectedItems = [.. expectedItemsList];

            int[] expectedMove_Event_OldItems = [itemMoved];
            var expectedMove_Event_OldStartingIndex = itemIndexOld;
            int[] expectedMove_Event_NewItems = [itemMoved];
            var expectedMove_Event_NewStartingIndex = itemIndexNew;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            collection.Move(itemIndexOld, itemIndexNew);

            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Move));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedMove_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedMove_Event_NewStartingIndex));
        }

        #endregion



        #region Clear

        [Test]
        public void CanClear_EmptyObservableCollection()
        {
            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Clear
            //
            collection.Clear();

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CanClear_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsRemoved = [.. items];
            var itemsRemovedCount = itemsRemoved.Length;

            //int[] expectedItems = [];

            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Clear
            //
            collection.Clear();

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        [Test]
        public void CanClear_NonEmptyObservableCollectionInReadOnlyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsRemoved = [.. items];
            var itemsRemovedCount = itemsRemoved.Length;

            //int[] expectedItems = [];

            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;
            var expectedReadOnlyEventCount = 1;
            var expectedReadOnlyPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

            Assert.That(readOnlyCollection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var readOnlyEvents = readOnlyCollection.CollectionChangedEventArgsList;
            var readOnlyPropertyChangedEvents = readOnlyCollection.PropertyChangedEventArgsList;

            Assert.That(readOnlyEvents, Has.Count.EqualTo(0));
            Assert.That(readOnlyPropertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Clear
            //
            collection.Clear();

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.Empty);

            Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
            Assert.That(readOnlyCollection, Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);

            Assert.That(readOnlyEvents, Has.Count.EqualTo(expectedReadOnlyEventCount));
            AssertPropertyChangedEvents(readOnlyPropertyChangedEvents, expectedReadOnlyPropertyChangedEventPropertyNames);

            var readOnlyEvent = readOnlyEvents[0];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(readOnlyEvent.OldItems, Is.Null);
            Assert.That(readOnlyEvent.NewItems, Is.Null);
        }

        #endregion



        #region AddRange

        [Test]
        public void CanAddRange_EmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsAdded = [.. items];
            var itemsAddedCount = itemsAdded.Length;

            int[] expectedItems = [.. itemsAdded];

            int[] expectedAddRange_Event_NewItems = [.. itemsAdded];
            var expectedAddRange_Event_NewStartingIndex = 0;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // AddRange
            //
            collection.AddRange(itemsAdded);

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedAddRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedAddRange_Event_NewStartingIndex));
        }

        [Test]
        public void CanAddRange_NonEmptyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            int[] itemsAdded = [.. items];
            var itemsAddedCount = itemsAdded.Length;

            int[] expectedItems = [.. items, .. itemsAdded];

            int[] expectedAddRange_Event_NewItems = [.. itemsAdded];
            var expectedAddRange_Event_NewStartingIndex = itemsCount;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // AddRange
            //
            collection.AddRange(itemsAdded);

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedAddRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedAddRange_Event_NewStartingIndex));
        }

        #endregion



        #region InsertRange

        [Test]
        public void CanInsertRange_EmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 0;
            int[] itemsInserted = [100, 101, 102];
            var itemsInsertedCount = itemsInserted.Length;

            int[] expectedItems = [.. itemsInserted];

            int[] expectedInsertRange_Event_NewItems = [.. itemsInserted];
            var expectedInsertRange_Event_NewStartingIndex = itemsInsertedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            collection.InsertRange(itemsInsertedIndex, itemsInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedInsertRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedInsertRange_Event_NewStartingIndex));
        }

        [Test]
        public void CanInsertRange_AtStart_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 0;
            int[] itemsInserted = [100, 101, 102];
            var itemsInsertedCount = itemsInserted.Length;

            var expectedItems = items.Take(itemsInsertedIndex)
                                     .Concat(itemsInserted)
                                     .Concat(items.Skip(itemsInsertedIndex))
                                     .ToArray();

            int[] expectedInsertRange_Event_NewItems = [.. itemsInserted];
            var expectedInsertRange_Event_NewStartingIndex = itemsInsertedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            collection.InsertRange(itemsInsertedIndex, itemsInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedInsertRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedInsertRange_Event_NewStartingIndex));
        }

        [Test]
        public void CanInsertRange_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 3;
            int[] itemsInserted = [100, 101, 102];
            var itemsInsertedCount = itemsInserted.Length;

            var expectedItems = items.Take(itemsInsertedIndex)
                                     .Concat(itemsInserted)
                                     .Concat(items.Skip(itemsInsertedIndex))
                                     .ToArray();

            int[] expectedInsertRange_Event_NewItems = [.. itemsInserted];
            var expectedInsertRange_Event_NewStartingIndex = itemsInsertedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            collection.InsertRange(itemsInsertedIndex, itemsInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedInsertRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedInsertRange_Event_NewStartingIndex));
        }

        [Test]
        public void CanInsertRange_AtEnd_NonEmptyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsInsertedIndex = itemsCount;
            int[] itemsInserted = [100, 101, 102];
            var itemsInsertedCount = itemsInserted.Length;

            var expectedItems = items.Take(itemsInsertedIndex)
                                     .Concat(itemsInserted)
                                     .Concat(items.Skip(itemsInsertedIndex))
                                     .ToArray();

            int[] expectedInsertRange_Event_NewItems = [.. itemsInserted];
            var expectedInsertRange_Event_NewStartingIndex = itemsInsertedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            collection.InsertRange(itemsInsertedIndex, itemsInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedInsertRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedInsertRange_Event_NewStartingIndex));
        }



        [Test]
        public void CanInsertRange_EmptyItems_EmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 0;
            int[] itemsInserted = [];
            var itemsInsertedCount = itemsInserted.Length;

            //int[] expectedItems = [];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            collection.InsertRange(itemsInsertedIndex, itemsInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CanInsertRange_EmptyItems_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 3;
            int[] itemsInserted = [];
            var itemsInsertedCount = itemsInserted.Length;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            collection.InsertRange(itemsInsertedIndex, itemsInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotInsertRange_NullCollection_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 3;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            Assert.Throws<ArgumentNullException>(
                () => collection.InsertRange(itemsInsertedIndex, null!));

            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotInsertRange_WithNegativeIndex_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = -1;
            int[] itemsInserted = [100, 101, 102];

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.InsertRange(itemsInsertedIndex, itemsInserted));

            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotInsertRange_WithIndexGreaterThanCount_NonEmptyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsInsertedIndex = itemsCount + 1;
            int[] itemsInserted = [100, 101, 102];

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.InsertRange(itemsInsertedIndex, itemsInserted));

            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        #endregion



        #region RemoveRange

        [Test]
        public void CanRemoveRange_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;
            //var itemsRemoved = items.GetRange(itemsRemovedIndex, itemsRemovedCount);
            var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));

            var expectedItems = items.Take(itemsRemovedIndex)
                                     .Concat(items.Skip(itemsRemovedIndex + itemsRemovedCount))
                                     .ToArray();

            int[] expectedRemoveRange_Event_OldItems = [.. itemsRemoved];
            var expectedRemoveRange_Event_OldStartingIndex = itemsRemovedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveRange
            //
            collection.RemoveRange(itemsRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveRange_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemoveRange_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanRemoveRange_All_NonEmptyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsRemovedIndex = 0;
            var itemsRemovedCount = itemsCount;
            //var itemsRemoved = items.GetRange(itemsRemovedIndex, itemsRemovedCount);
            var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));

            var expectedItems = items.Take(itemsRemovedIndex)
                                     .Concat(items.Skip(itemsRemovedIndex + itemsRemovedCount))
                                     .ToArray();

            //int[] expectedRemoveRange_Event_OldItems = [.. itemsRemoved];
            //var expectedRemoveRange_Event_OldStartingIndex = itemsRemovedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveRange
            //
            collection.RemoveRange(itemsRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        [Test]
        public void CanRemoveRange_ByIndexAndCount_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;
            //var itemsRemoved = items.GetRange(itemsRemovedIndex, itemsRemovedCount);
            var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));

            var expectedItems = items.Take(itemsRemovedIndex)
                                     .Concat(items.Skip(itemsRemovedIndex + itemsRemovedCount))
                                     .ToArray();

            int[] expectedRemoveRange_Event_OldItems = [.. itemsRemoved];
            var expectedRemoveRange_Event_OldStartingIndex = itemsRemovedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveRange (By Index and Count)
            //
            collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveRange_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemoveRange_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanRemoveRange_ByIndexAndCount_All_NonEmptyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsRemovedIndex = 0;
            var itemsRemovedCount = itemsCount;
            //var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));

            var expectedItems = items.Take(itemsRemovedIndex)
                                     .Concat(items.Skip(itemsRemovedIndex + itemsRemovedCount))
                                     .ToArray();

            //int[] expectedRemoveRange_Event_OldItems = [.. itemsRemoved];
            //var expectedRemoveRange_Event_OldStartingIndex = itemsRemovedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveRange (By Index and Count)
            //
            collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        #endregion



        #region RemoveWhere

        [Test]
        public void CanRemoveWhere_WithSingleCluster_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [0, 1, 2, 6, 7, 8, 9];
            var expectedItemsRemovedCount = 3;

            int[] expectedRemove_Event_OldItems = [3, 4, 5];
            var expectedRemove_Event_OldItemsCount = expectedRemove_Event_OldItems.Length;
            var expectedRemove_Event_OldStartingIndex = 3;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var itemsRemovedCount_RemoveWhere = collection.RemoveWhere(item => item is >= 3 and <= 5);

            Assert.That(itemsRemovedCount_RemoveWhere, Is.EqualTo(expectedItemsRemovedCount));

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemove_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanRemoveWhere_WithMultipleClusters_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [0, 3, 4, 7, 8, 9];
            var expectedItemsRemovedCount = 4;

            int[] expectedRemove_Event_0_OldItems = [1, 2];
            var expectedRemove_Event_0_OldItemsCount = expectedRemove_Event_0_OldItems.Length;
            var expectedRemove_Event_0_OldStartingIndex = 1;

            int[] expectedRemove_Event_1_OldItems = [5, 6];
            var expectedRemove_Event_1_OldItemsCount = expectedRemove_Event_1_OldItems.Length;
            var expectedRemove_Event_1_OldStartingIndex = 3;

            var expectedEventCount = 2;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var itemsRemovedCount_RemoveWhere = collection.RemoveWhere(item => item is 1 or 2 or 5 or 6);

            Assert.That(itemsRemovedCount_RemoveWhere, Is.EqualTo(expectedItemsRemovedCount));

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var event_0 = events[0];

            Assert.That(event_0, Is.Not.Null);
            Assert.That(event_0.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var event_0_OldItems = event_0.OldItems;

            Assert.That(event_0_OldItems, Is.Not.Null);
            Assert.That(event_0_OldItems, Has.Count.EqualTo(expectedRemove_Event_0_OldItemsCount));
            Assert.That(event_0_OldItems!.Cast<int>(), Is.EqualTo(expectedRemove_Event_0_OldItems));
            Assert.That(event_0.OldStartingIndex, Is.EqualTo(expectedRemove_Event_0_OldStartingIndex));
            Assert.That(event_0.NewItems, Is.Null);

            var event_1 = events[1];

            Assert.That(event_1, Is.Not.Null);
            Assert.That(event_1.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var event_1_OldItems = event_1.OldItems;

            Assert.That(event_1_OldItems, Is.Not.Null);
            Assert.That(event_1_OldItems, Has.Count.EqualTo(expectedRemove_Event_1_OldItemsCount));
            Assert.That(event_1_OldItems!.Cast<int>(), Is.EqualTo(expectedRemove_Event_1_OldItems));
            Assert.That(event_1.OldStartingIndex, Is.EqualTo(expectedRemove_Event_1_OldStartingIndex));
            Assert.That(event_1.NewItems, Is.Null);
        }

        [Test]
        public void CanRemoveWhere_AllItems_NonEmptyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var expectedItemsRemovedCount = itemsCount;

            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var itemsRemovedCount_RemoveWhere = collection.RemoveWhere(item => true);

            Assert.That(itemsRemovedCount_RemoveWhere, Is.EqualTo(expectedItemsRemovedCount));

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        [Test]
        public void CannotRemoveWhere_WithNoMatch_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];
            var expectedItemsRemovedCount = 0;

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var itemsRemovedCount_RemoveWhere = collection.RemoveWhere(item => item < 0);

            Assert.That(itemsRemovedCount_RemoveWhere, Is.EqualTo(expectedItemsRemovedCount));

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotRemoveWhere_EmptyObservableCollection()
        {
            var expectedItemsRemovedCount = 0;

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var itemsRemovedCount_RemoveWhere = collection.RemoveWhere(item => true);

            Assert.That(itemsRemovedCount_RemoveWhere, Is.EqualTo(expectedItemsRemovedCount));

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotRemoveWhere_WithNullMatch_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            Predicate<int>? match = null;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "match";

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => collection.RemoveWhere(match!));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotRemoveWhere_WhenMatchThrows_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var exception =
                Assert.Throws<InvalidOperationException>(
                    () => collection.RemoveWhere(
                        item =>
                        {
                            if (item == 5)
                            {
                                throw new InvalidOperationException();
                            }

                            return item is >= 2 and <= 7;
                        }));

            Assert.That(exception, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(2), Is.True);
            Assert.That(collection.Contains(3), Is.True);
            Assert.That(collection.Contains(4), Is.True);
            Assert.That(collection.Contains(5), Is.True);
            Assert.That(collection.Contains(6), Is.True);
            Assert.That(collection.Contains(7), Is.True);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        #endregion



        #region ReplaceRange

        [Test]
        public void CanReplaceRange_NonEmptyObservableCollection()
        {
            // 2,6,[0,1,2,3,4,5]
            // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
            // After:   0,1,0,1,2,3,4,5,8,9                 (10)

            var items = _items;
            //var itemsCount = items.Count;

            var itemsReplacedIndex = 2;
            var itemsReplacedCount = 6;
            //var itemsReplacement = items.GetRange(0, 6);
            var itemsReplacement = items.GetRange(0..6);
            var itemsReplacementCount = itemsReplacement.Count;

            var expectedItems = items.Take(itemsReplacedIndex)
                                     .Concat(itemsReplacement)
                                     .Concat(items.Skip(itemsReplacedIndex + itemsReplacedCount))
                                     .ToArray();
            //int[] expectedItems = [0, 1, 0, 1, 2, 3, 4, 5, 8, 9];

            int[] expectedReplaceRange_Event_OldItems = [2, 3, 4, 5, 6, 7];
            var expectedReplaceRange_Event_OldStartingIndex = itemsReplacedIndex;
            int[] expectedReplaceRange_Event_NewItems = [0, 1, 2, 3, 4, 5];
            var expectedReplaceRange_Event_NewStartingIndex = itemsReplacedIndex;
            //var expectedReplace_Event_Count = expectedReplaceRange_Event_OldItems.Length;
            var expectedEventCount = 1; // 1 (ReplaceRange)
            //var expectedEventCount = 6; // 6 (Replace)
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ReplaceRange
            //
            collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement);

            collectionCount = collectionCount - itemsReplacedCount + itemsReplacementCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            NotifyCollectionChangedEventArgs @event;
            IList? eventOldItems;
            IList? eventNewItems;



            //
            // ReplaceRange Events
            //
            @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedReplaceRange_Event_OldStartingIndex));

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedReplaceRange_Event_NewStartingIndex));



            //
            // Replace Events
            //
            //for (var i = 0; i < expectedReplace_Event_Count; i++)
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
            //    Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo([expectedReplaceRange_Event_OldItems[i]]));
            //    Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedReplaceRange_Event_OldStartingIndex + i));
            //
            //    eventNewItems = @event.NewItems;
            //
            //    Assert.That(eventNewItems, Is.Not.Null);
            //    Assert.That(eventNewItems, Has.Count.EqualTo(1));
            //    Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo([expectedReplaceRange_Event_NewItems[i]]));
            //    Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedReplaceRange_Event_NewStartingIndex + i));
            //}
        }

        [Test]
        public void CanReplaceRange_WithAddRange_NonEmptyObservableCollection()
        {
            // 0,4,[0,1,2,3,4,5,6,7,8,9]
            // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
            // After:   0,1,2,3,4,5,6,7,8,9,4,5,6,7,8,9     (16)
            //
            // 4,4,[0,1,2,3,4,5,6,7,8,9]
            // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
            // After:   0,1,2,3,0,1,2,3,4,5,6,7,8,9,8,9     (16)
            //
            // 5,4,[0,1,2,3,4,5,6,7,8,9]
            // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
            // After:   0,1,2,3,4,0,1,2,3,4,5,6,7,8,9,9     (16)
            //
            // 6,4,[0,1,2,3,4,5,6,7,8,9]
            // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
            // After:   0,1,2,3,4,5,0,1,2,3,4,5,6,7,8,9     (16)

            var items = _items;
            var itemsCount = items.Count;

            var itemsReplacedIndex = 4;
            var itemsReplacedCount = 4;
            //var itemsReplacement = items.GetRange(0, items.Count);
            var itemsReplacement = items.GetRange(0..);
            var itemsReplacementCount = itemsReplacement.Count;

            var expectedItems = items.Take(itemsReplacedIndex)
                                     .Concat(itemsReplacement)
                                     .Concat(items.Skip(itemsReplacedIndex + itemsReplacedCount))
                                     .ToArray();
            //int[] expectedItems = [0, 1, 2, 3, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 9];

            int[] expectedReplaceRange_Event_OldItems = [4, 5, 6, 7, 8, 9];
            var expectedReplaceRange_Event_OldStartingIndex = itemsReplacedIndex;
            int[] expectedReplaceRange_Event_NewItems = [0, 1, 2, 3, 4, 5];
            var expectedReplaceRange_Event_NewStartingIndex = itemsReplacedIndex;
            //var expectedReplace_Event_Count = expectedReplaceRange_Event_OldItems.Length;
            int[] expectedAddRange_Event_NewItems = [6, 7, 8, 9, 8, 9];
            var expectedAddRange_Event_NewStartingIndex = itemsCount;
            var expectedEventCount = 2; // 1 (ReplaceRange) + 1 (AddRange)
            //var expectedEventCount = 7; // 6 (Replace) + 1 (AddRange)
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ReplaceRange
            //
            collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement);

            collectionCount = collectionCount - itemsReplacedCount + itemsReplacementCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            NotifyCollectionChangedEventArgs @event;
            IList? eventOldItems;
            IList? eventNewItems;



            //
            // ReplaceRange Events
            //
            @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedReplaceRange_Event_OldStartingIndex));

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedReplaceRange_Event_NewStartingIndex));



            //
            // Replace Events
            //
            //for (var i = 0; i < expectedReplace_Event_Count; i++)
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
            //    Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo([expectedReplaceRange_Event_OldItems[i]]));
            //    Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedReplaceRange_Event_OldStartingIndex + i));
            //
            //    eventNewItems = @event.NewItems;
            //
            //    Assert.That(eventNewItems, Is.Not.Null);
            //    Assert.That(eventNewItems, Has.Count.EqualTo(1));
            //    Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo([expectedReplaceRange_Event_NewItems[i]]));
            //    Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedReplaceRange_Event_NewStartingIndex + i));
            //}



            //
            // AddRange Events
            //
            @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedAddRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedAddRange_Event_NewStartingIndex));
        }

        [Test]
        public void CanReplaceRange_WithRemoveRange_NonEmptyObservableCollection()
        {
            // 0,6,[0,1]
            // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
            // After:   0,1,6,7,8,9                          (6)
            //
            // 4,6,[0,1]
            // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
            // After:   0,1,2,3,0,1                          (6)
            //
            // 8,4,[0,1]
            // Before:  0,1,2,3,4,5,6,7,8,9                 (10)
            // After:   0,1,2,3,4,5,6,7,0,1                  (6)

            var items = _items;
            //var itemsCount = items.Count;

            var itemsReplacedIndex = 4;
            var itemsReplacedCount = 6;
            //var itemsReplacement = items.GetRange(0, 2);    // 2 items
            var itemsReplacement = items.GetRange(0..2);    // 2 items
            var itemsReplacementCount = itemsReplacement.Count;

            var expectedItems = items.Take(itemsReplacedIndex)
                                     .Concat(itemsReplacement)
                                     .Concat(items.Skip(itemsReplacedIndex + itemsReplacedCount))
                                     .ToArray();
            //int[] expectedItems = [0, 1, 2, 3, 0, 1];

            int[] expectedReplaceRange_Event_OldItems = [4, 5];
            var expectedReplaceRange_Event_OldStartingIndex = itemsReplacedIndex;
            int[] expectedReplaceRange_Event_NewItems = [0, 1];
            var expectedReplaceRange_Event_NewStartingIndex = itemsReplacedIndex;
            //var expectedReplace_Event_Count = expectedReplaceRange_Event_OldItems.Length;
            int[] expectedRemoveRange_Event_OldItems = [6, 7, 8, 9];
            //var expectedRemoveRange_Event_OldStartingIndex = itemsCount - itemsReplacedCount + itemsReplacementCount;
            var expectedRemoveRange_Event_OldStartingIndex = expectedItems.Length;
            var expectedEventCount = 2; // 1 (ReplaceRange) + 1 (RemoveRange)
            //var expectedEventCount = 3; // 2 (Replace) + 1 (RemoveRange)
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ReplaceRange
            //
            collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement);

            collectionCount = collectionCount - itemsReplacedCount + itemsReplacementCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            NotifyCollectionChangedEventArgs @event;
            IList? eventOldItems;
            IList? eventNewItems;



            //
            // ReplaceRange Events
            //
            @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedReplaceRange_Event_OldStartingIndex));

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedReplaceRange_Event_NewStartingIndex));



            //
            // Replace Events
            //
            //for (var i = 0; i < expectedReplace_Event_Count; i++)
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
            //    Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo([expectedReplaceRange_Event_OldItems[i]]));
            //    Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedReplaceRange_Event_OldStartingIndex + i));
            //
            //    eventNewItems = @event.NewItems;
            //
            //    Assert.That(eventNewItems, Is.Not.Null);
            //    Assert.That(eventNewItems, Has.Count.EqualTo(1));
            //    Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo([expectedReplaceRange_Event_NewItems[i]]));
            //    Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedReplaceRange_Event_NewStartingIndex + i));
            //}



            //
            // RemoveRange Events
            //
            @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveRange_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemoveRange_Event_OldStartingIndex));

            eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }



        [Test]
        public void CanReplaceRange_WithEmptyReplacement_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsReplacedIndex = 3;
            var itemsReplacedCount = 4;
            int[] itemsReplacement = [];
            var itemsReplacementCount = itemsReplacement.Length;

            var expectedItems = items.Take(itemsReplacedIndex)
                                     .Concat(itemsReplacement)
                                     .Concat(items.Skip(itemsReplacedIndex + itemsReplacedCount))
                                     .ToArray();

            int[] expectedRemoveRange_Event_OldItems = [3, 4, 5, 6];
            var expectedRemoveRange_Event_OldStartingIndex = itemsReplacedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ReplaceRange
            //
            collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement);

            collectionCount = collectionCount - itemsReplacedCount + itemsReplacementCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveRange_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemoveRange_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanReplaceRange_WithEmptyReplacement_All_NonEmptyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsReplacedIndex = 0;
            var itemsReplacedCount = itemsCount;
            int[] itemsReplacement = [];
            var itemsReplacementCount = itemsReplacement.Length;

            var expectedItems = items.Take(itemsReplacedIndex)
                                     .Concat(itemsReplacement)
                                     .Concat(items.Skip(itemsReplacedIndex + itemsReplacedCount))
                                     .ToArray();

            //int[] expectedRemoveRange_Event_OldItems = [.. items];
            //var expectedRemoveRange_Event_OldStartingIndex = itemsReplacedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ReplaceRange
            //
            collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement);

            collectionCount = collectionCount - itemsReplacedCount + itemsReplacementCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        #endregion



        #region Range No-Op

        [Test]
        public void CanAddRange_EmptyItems_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsAdded = [];
            var itemsAddedCount = itemsAdded.Length;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // AddRange
            //
            collection.AddRange(itemsAdded);

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CanRemoveRange_EmptyItems_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsRemoved = [];
            var itemsRemovedCount = itemsRemoved.Length;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveRange
            //
            collection.RemoveRange(itemsRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CanRemoveRange_NonExistingItems_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsRemoved = [100, 101, 102];
            var itemsRemovedCount = 0;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveRange
            //
            collection.RemoveRange(itemsRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CanRemoveRange_ByIndexAndCount_ZeroCount_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 0;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveRange (By Index and Count)
            //
            collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CanRemoveRange_ByIndexAndCount_ZeroCountAtEnd_NonEmptyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsRemovedIndex = itemsCount;
            var itemsRemovedCount = 0;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveRange (By Index and Count)
            //
            collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CanReplaceRange_ZeroCountWithReplacement_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsReplacedIndex = 3;
            var itemsReplacedCount = 0;
            int[] itemsReplacement = [100, 101, 102];
            //var itemsReplacementCount = itemsReplacement.Length;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ReplaceRange
            //
            collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement);

            // Count 0 means no target range, therefore no operation.
            // Replacement items must not be inserted.
            collectionCount = collectionCount - itemsReplacedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        #endregion



        #region Reentrancy

        [Test]
        public void CanSetItem_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemSetIndex = 3;
            var itemSet = 100;

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            var expectedItems = items.ToArray();

            expectedItems[itemSetIndex] = itemSet;
            expectedItems = [.. expectedItems, itemAddedReentrantly];

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // SetItem
            //
            Assert.DoesNotThrow(
                () => collection[itemSetIndex] = itemSet);

            collectionCount += itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanSetItem_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemSetIndex = 3;
            var itemSet = 100;

            var itemAddedReentrantly = 999;

            var expectedItems = items.ToArray();

            expectedItems[itemSetIndex] = itemSet;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // SetItem
            //
            Assert.DoesNotThrow(
                () => collection[itemSetIndex] = itemSet);

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemSet), Is.True);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotSetItem_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemSetIndex = 3;
            var itemSet = 100;

            var itemAddedReentrantly = 999;

            var expectedItems = items.ToArray();

            expectedItems[itemSetIndex] = itemSet;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // SetItem
            //
            var exception =
                Assert.Throws<InvalidOperationException>(
                    () => collection[itemSetIndex] = itemSet);

            Assert.That(exception, Is.Not.Null);

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.False);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemSet), Is.True);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }



        [Test]
        public void CanAdd_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAdded = 100;
            var itemsAddedCount = 1;

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            var expectedItems = items.Concat([itemAdded])
                                     .Concat([itemAddedReentrantly])
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // Add
            //
            Assert.DoesNotThrow(
                () => collection.Add(itemAdded));

            collectionCount += itemsAddedCount + itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanAdd_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAdded = 100;

            var itemAddedReentrantly = 999;

            var expectedItems = items.Concat([itemAdded])
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Add
            //
            Assert.DoesNotThrow(
                () => collection.Add(itemAdded));

            collectionCount++;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemAdded), Is.True);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotAdd_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAdded = 100;

            var itemAddedReentrantly = 999;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Add
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.Add(itemAdded));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanInsert_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = 3;
            var itemInserted = 100;

            var itemInsertedReentrantlyIndex = 4;
            var itemInsertedReentrantly = 999;
            var itemsInsertedReentrantlyCount = 1;

            var expectedItems = items.Take(itemInsertedIndex)
                                     .Concat([itemInserted])
                                     .Concat([itemInsertedReentrantly])
                                     .Concat(items.Skip(itemInsertedIndex))
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Insert(itemInsertedReentrantlyIndex, itemInsertedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // Insert
            //
            Assert.DoesNotThrow(
                () => collection.Insert(itemInsertedIndex, itemInserted));

            collectionCount += 1 + itemsInsertedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanInsert_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = 3;
            var itemInserted = 100;

            var itemInsertedReentrantlyIndex = 4;
            var itemInsertedReentrantly = 999;

            var expectedItems = items.Take(itemInsertedIndex)
                                     .Concat([itemInserted])
                                     .Concat(items.Skip(itemInsertedIndex))
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Insert(itemInsertedReentrantlyIndex, itemInsertedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Insert
            //
            Assert.DoesNotThrow(
                () => collection.Insert(itemInsertedIndex, itemInserted));

            collectionCount++;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemInserted), Is.True);
            Assert.That(collection.Contains(itemInsertedReentrantly), Is.False);
        }

        [Test]
        public void CannotInsert_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = 3;
            var itemInserted = 100;

            var itemInsertedReentrantlyIndex = 4;
            var itemInsertedReentrantly = 999;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Insert(itemInsertedReentrantlyIndex, itemInsertedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Insert
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.Insert(itemInsertedIndex, itemInserted));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanRemove_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemRemovedReentrantlyIndex = 4;
            var itemRemovedReentrantly = items[itemRemovedReentrantlyIndex];

            var expectedItems = items.Where(item => item != itemRemoved && item != itemRemovedReentrantly)
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Remove(itemRemovedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // Remove
            //
            Assert.DoesNotThrow(
                () => collection.Remove(itemRemoved));

            collectionCount -= 2;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemRemoved), Is.False);
            Assert.That(collection.Contains(itemRemovedReentrantly), Is.False);
        }

        [Test]
        public void CanRemove_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemRemovedReentrantlyIndex = 4;
            var itemRemovedReentrantly = items[itemRemovedReentrantlyIndex];

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Remove(itemRemovedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Remove
            //
            Assert.DoesNotThrow(
                () => collection.Remove(itemRemoved));

            collectionCount--;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemRemoved), Is.False);
            Assert.That(collection.Contains(itemRemovedReentrantly), Is.True);
        }

        [Test]
        public void CannotRemove_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemRemovedReentrantlyIndex = 4;
            var itemRemovedReentrantly = items[itemRemovedReentrantlyIndex];

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Remove(itemRemovedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Remove
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.Remove(itemRemoved));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanRemoveAt_ByIndex_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemRemovedReentrantlyIndex = 3;
            var itemRemovedReentrantly = items[itemRemovedIndex + 1];

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex && index != itemRemovedIndex + 1)
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.RemoveAt(itemRemovedReentrantlyIndex);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // RemoveAt (By Index)
            //
            Assert.DoesNotThrow(
                () => collection.RemoveAt(itemRemovedIndex));

            collectionCount -= 2;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemRemoved), Is.False);
            Assert.That(collection.Contains(itemRemovedReentrantly), Is.False);
        }

        [Test]
        public void CanRemoveAt_ByIndex_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemRemovedReentrantlyIndex = 4;
            var itemRemovedReentrantly = items[itemRemovedReentrantlyIndex];

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.RemoveAt(itemRemovedReentrantlyIndex);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // RemoveAt (By Index)
            //
            Assert.DoesNotThrow(
                () => collection.RemoveAt(itemRemovedIndex));

            collectionCount--;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemRemoved), Is.False);
            Assert.That(collection.Contains(itemRemovedReentrantly), Is.True);
        }

        [Test]
        public void CannotRemoveAt_ByIndex_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;

            var itemRemovedReentrantlyIndex = 4;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.RemoveAt(itemRemovedReentrantlyIndex);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // RemoveAt (By Index)
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.RemoveAt(itemRemovedIndex));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanMove_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemMovedIndexOld = 3;
            var itemMovedIndexNew = 4;

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            var expectedItems = items.ToList();

            var itemMoved = expectedItems[itemMovedIndexOld];

            expectedItems.RemoveAt(itemMovedIndexOld);
            expectedItems.Insert(itemMovedIndexNew, itemMoved);
            expectedItems.Add(itemAddedReentrantly);

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // Move
            //
            Assert.DoesNotThrow(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            collectionCount += itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanMove_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemMovedIndexOld = 3;
            var itemMovedIndexNew = 4;

            var itemAddedReentrantly = 999;

            var expectedItems = items.ToList();

            var itemMoved = expectedItems[itemMovedIndexOld];

            expectedItems.RemoveAt(itemMovedIndexOld);
            expectedItems.Insert(itemMovedIndexNew, itemMoved);

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Move
            //
            Assert.DoesNotThrow(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotMove_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemMovedIndexOld = 3;
            var itemMovedIndexNew = 4;

            var itemMovedReentrantlyIndexOld = 5;
            var itemMovedReentrantlyIndexNew = 6;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Move(itemMovedReentrantlyIndexOld, itemMovedReentrantlyIndexNew);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Move
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanClear_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            int[] expectedItems = [itemAddedReentrantly];

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // Clear
            //
            Assert.DoesNotThrow(
                collection.Clear);

            collectionCount = itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanClear_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Clear
            //
            Assert.DoesNotThrow(
                collection.Clear);

            collectionCount = 0;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.Empty);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotClear_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // Clear
            //
            var exception = Assert.Throws<InvalidOperationException>(
                collection.Clear);

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanAddRange_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsAdded = [100, 101, 102];
            var itemsAddedCount = itemsAdded.Length;

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            var expectedItems = items.Concat(itemsAdded)
                                     .Concat([itemAddedReentrantly])
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // AddRange
            //
            Assert.DoesNotThrow(
                () => collection.AddRange(itemsAdded));

            collectionCount += itemsAddedCount + itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanAddRange_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsAdded = [100, 101, 102];
            var itemsAddedCount = itemsAdded.Length;

            var itemAddedReentrantly = 999;

            var expectedItems = items.Concat(itemsAdded)
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // AddRange
            //
            Assert.DoesNotThrow(
                () => collection.AddRange(itemsAdded));

            collectionCount += itemsAddedCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotAddRange_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsAdded = [100, 101, 102];

            var itemAddedReentrantly = 999;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // AddRange
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.AddRange(itemsAdded));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanInsertRange_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 3;
            int[] itemsInserted = [100, 101, 102];
            var itemsInsertedCount = itemsInserted.Length;

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            var expectedItems = items.Take(itemsInsertedIndex)
                                     .Concat(itemsInserted)
                                     .Concat(items.Skip(itemsInsertedIndex))
                                     .Concat([itemAddedReentrantly])
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // InsertRange
            //
            Assert.DoesNotThrow(
                () => collection.InsertRange(itemsInsertedIndex, itemsInserted));

            collectionCount += itemsInsertedCount + itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanInsertRange_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 3;
            int[] itemsInserted = [100, 101, 102];
            var itemsInsertedCount = itemsInserted.Length;

            var itemAddedReentrantly = 999;

            var expectedItems = items.Take(itemsInsertedIndex)
                                     .Concat(itemsInserted)
                                     .Concat(items.Skip(itemsInsertedIndex))
                                     .ToArray();

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // InsertRange
            //
            Assert.DoesNotThrow(
                () => collection.InsertRange(itemsInsertedIndex, itemsInserted));

            collectionCount += itemsInsertedCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotInsertRange_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 3;
            int[] itemsInserted = [100, 101, 102];

            var itemAddedReentrantly = 999;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // InsertRange
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.InsertRange(itemsInsertedIndex, itemsInserted));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanRemoveRange_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;
            var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            int[] expectedItems = [0, 1, 2, 7, 8, 9, itemAddedReentrantly];

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // RemoveRange
            //
            Assert.DoesNotThrow(
                () => collection.RemoveRange(itemsRemoved));

            collectionCount -= itemsRemovedCount;
            collectionCount += itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanRemoveRange_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;
            var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 2, 7, 8, 9];

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // RemoveRange
            //
            Assert.DoesNotThrow(
                () => collection.RemoveRange(itemsRemoved));

            collectionCount -= itemsRemovedCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotRemoveRange_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;
            var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));

            var itemAddedReentrantly = 999;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // RemoveRange
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.RemoveRange(itemsRemoved));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanRemoveRange_ByIndexAndCount_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            int[] expectedItems = [0, 1, 2, 7, 8, 9, itemAddedReentrantly];

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // RemoveRange (By Index and Count)
            //
            Assert.DoesNotThrow(
                () => collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount));

            collectionCount -= itemsRemovedCount;
            collectionCount += itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanRemoveRange_ByIndexAndCount_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 2, 7, 8, 9];

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // RemoveRange (By Index and Count)
            //
            Assert.DoesNotThrow(
                () => collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount));

            collectionCount -= itemsRemovedCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotRemoveRange_ByIndexAndCount_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;

            var itemAddedReentrantly = 999;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // RemoveRange (By Index and Count)
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }



        [Test]
        public void CanRemoveWhere_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 2, 6, 7, 8, 9, itemAddedReentrantly];
            var expectedItemsRemovedCount = 3;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // RemoveWhere
            //
            var itemsRemovedCount_RemoveWhere = collection.RemoveWhere(item => item is >= 3 and <= 5);

            Assert.That(itemsRemovedCount_RemoveWhere, Is.EqualTo(expectedItemsRemovedCount));
            Assert.That(hasReentered, Is.True);

            collectionCount -= expectedItemsRemovedCount;
            collectionCount++;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(3), Is.False);
            Assert.That(collection.Contains(4), Is.False);
            Assert.That(collection.Contains(5), Is.False);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.True);
        }

        [Test]
        public void CanRemoveWhere_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 2, 6, 7, 8, 9];
            var expectedItemsRemovedCount = 3;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // RemoveWhere
            //
            var itemsRemovedCount_RemoveWhere = collection.RemoveWhere(item => item is >= 3 and <= 5);

            Assert.That(itemsRemovedCount_RemoveWhere, Is.EqualTo(expectedItemsRemovedCount));

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(3), Is.False);
            Assert.That(collection.Contains(4), Is.False);
            Assert.That(collection.Contains(5), Is.False);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotRemoveWhere_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 2, 6, 7, 8, 9];

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // RemoveWhere
            //
            var exception =
                Assert.Throws<InvalidOperationException>(
                    () => collection.RemoveWhere(item => item is >= 3 and <= 5));

            Assert.That(exception, Is.Not.Null);

            collectionCount -= 3;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.False);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(3), Is.False);
            Assert.That(collection.Contains(4), Is.False);
            Assert.That(collection.Contains(5), Is.False);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }



        [Test]
        public void CanReplaceRange_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsReplacedIndex = 3;
            var itemsReplacedCount = 4;
            int[] itemsReplacement = [100, 101, 102];

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            int[] expectedItems = [0, 1, 2, 100, 101, 102, 7, 8, 9, itemAddedReentrantly];

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            collection.CollectionChanged += reentrantHandler;

            //
            // ReplaceRange
            //
            Assert.DoesNotThrow(
                () => collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement));

            collectionCount -= itemsReplacedCount;
            collectionCount += itemsReplacement.Length;
            collectionCount += itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanReplaceRange_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsReplacedIndex = 3;
            var itemsReplacedCount = 4;
            int[] itemsReplacement = [100, 101, 102];

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 2, 100, 101, 102, 7, 8, 9];

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;
            var hasObserved = false;
            InvalidOperationException? reentrantException = null;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    try
                    {
                        collection.Add(itemAddedReentrantly);
                    }
                    catch (InvalidOperationException exception)
                    {
                        reentrantException = exception;
                    }
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                    hasObserved = true;
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // ReplaceRange
            //
            Assert.DoesNotThrow(
                () => collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement));

            collectionCount -= itemsReplacedCount;
            collectionCount += itemsReplacement.Length;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotReplaceRange_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsReplacedIndex = 3;
            var itemsReplacedCount = 4;
            int[] itemsReplacement = [100, 101, 102];

            var itemAddedReentrantly = 999;

            var collection = new TestObservableCollection<int>(items);

            Assert.That(collection, Is.EqualTo(items));

            var hasReentered = false;

            NotifyCollectionChangedEventHandler reentrantHandler =
                (sender, args) =>
                {
                    if (hasReentered)
                    {
                        return;
                    }

                    hasReentered = true;

                    collection.Add(itemAddedReentrantly);
                };

            NotifyCollectionChangedEventHandler observerHandler =
                (sender, args) =>
                {
                };

            collection.CollectionChanged += reentrantHandler;
            collection.CollectionChanged += observerHandler;

            //
            // ReplaceRange
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement));

            Assert.That(exception, Is.Not.Null);
            Assert.That(hasReentered, Is.True);
        }

        #endregion



        #region ReadOnlyObservableCollection Range Forwarding

        [Test]
        public void CanAddRange_EmptyObservableCollectionInReadOnlyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsAdded = [.. items];
            var itemsAddedCount = itemsAdded.Length;

            int[] expectedItems = [.. itemsAdded];

            int[] expectedAddRange_Event_NewItems = [.. itemsAdded];
            var expectedAddRange_Event_NewStartingIndex = 0;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;
            var expectedReadOnlyEventCount = expectedEventCount;
            var expectedReadOnlyPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);

            var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

            Assert.That(readOnlyCollection, Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var readOnlyEvents = readOnlyCollection.CollectionChangedEventArgsList;
            var readOnlyPropertyChangedEvents = readOnlyCollection.PropertyChangedEventArgsList;

            Assert.That(readOnlyEvents, Has.Count.EqualTo(0));
            Assert.That(readOnlyPropertyChangedEvents, Has.Count.EqualTo(0));

            //
            // AddRange
            //
            collection.AddRange(itemsAdded);

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
            Assert.That(readOnlyCollection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            Assert.That(readOnlyEvents, Has.Count.EqualTo(expectedReadOnlyEventCount));
            AssertPropertyChangedEvents(readOnlyPropertyChangedEvents, expectedReadOnlyPropertyChangedEventPropertyNames);

            var readOnlyEvent = readOnlyEvents[0];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var readOnlyEventOldItems = readOnlyEvent.OldItems;

            Assert.That(readOnlyEventOldItems, Is.Null);

            var readOnlyEventNewItems = readOnlyEvent.NewItems;

            Assert.That(readOnlyEventNewItems, Is.Not.Null);
            Assert.That(readOnlyEventNewItems!.Cast<int>(), Is.EqualTo(expectedAddRange_Event_NewItems));
            Assert.That(readOnlyEvent.NewStartingIndex, Is.EqualTo(expectedAddRange_Event_NewStartingIndex));
        }



        [Test]
        public void CanInsertRange_NonEmptyObservableCollectionInReadOnlyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsInsertedIndex = 3;
            int[] itemsInserted = [100, 101, 102];
            var itemsInsertedCount = itemsInserted.Length;

            var expectedItems = items.Take(itemsInsertedIndex)
                                     .Concat(itemsInserted)
                                     .Concat(items.Skip(itemsInsertedIndex))
                                     .ToArray();

            int[] expectedInsertRange_Event_NewItems = [.. itemsInserted];
            var expectedInsertRange_Event_NewStartingIndex = itemsInsertedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;
            var expectedReadOnlyEventCount = expectedEventCount;
            var expectedReadOnlyPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

            Assert.That(readOnlyCollection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var readOnlyEvents = readOnlyCollection.CollectionChangedEventArgsList;
            var readOnlyPropertyChangedEvents = readOnlyCollection.PropertyChangedEventArgsList;

            Assert.That(readOnlyEvents, Has.Count.EqualTo(0));
            Assert.That(readOnlyPropertyChangedEvents, Has.Count.EqualTo(0));

            //
            // InsertRange
            //
            collection.InsertRange(itemsInsertedIndex, itemsInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
            Assert.That(readOnlyCollection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            Assert.That(readOnlyEvents, Has.Count.EqualTo(expectedReadOnlyEventCount));
            AssertPropertyChangedEvents(readOnlyPropertyChangedEvents, expectedReadOnlyPropertyChangedEventPropertyNames);

            var readOnlyEvent = readOnlyEvents[0];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var readOnlyEventOldItems = readOnlyEvent.OldItems;

            Assert.That(readOnlyEventOldItems, Is.Null);

            var readOnlyEventNewItems = readOnlyEvent.NewItems;

            Assert.That(readOnlyEventNewItems, Is.Not.Null);
            Assert.That(readOnlyEventNewItems!.Cast<int>(), Is.EqualTo(expectedInsertRange_Event_NewItems));
            Assert.That(readOnlyEvent.NewStartingIndex, Is.EqualTo(expectedInsertRange_Event_NewStartingIndex));
        }



        [Test]
        public void CanRemoveRange_NonEmptyObservableCollectionInReadOnlyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;
            var itemsRemoved = items.GetRange(itemsRemovedIndex..(itemsRemovedIndex + itemsRemovedCount));

            var expectedItems = items.Take(itemsRemovedIndex)
                                     .Concat(items.Skip(itemsRemovedIndex + itemsRemovedCount))
                                     .ToArray();

            int[] expectedRemoveRange_Event_OldItems = [.. itemsRemoved];
            var expectedRemoveRange_Event_OldStartingIndex = itemsRemovedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;
            var expectedReadOnlyEventCount = expectedEventCount;
            var expectedReadOnlyPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

            Assert.That(readOnlyCollection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var readOnlyEvents = readOnlyCollection.CollectionChangedEventArgsList;
            var readOnlyPropertyChangedEvents = readOnlyCollection.PropertyChangedEventArgsList;

            Assert.That(readOnlyEvents, Has.Count.EqualTo(0));
            Assert.That(readOnlyPropertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveRange
            //
            collection.RemoveRange(itemsRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
            Assert.That(readOnlyCollection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            Assert.That(readOnlyEvents, Has.Count.EqualTo(expectedReadOnlyEventCount));
            AssertPropertyChangedEvents(readOnlyPropertyChangedEvents, expectedReadOnlyPropertyChangedEventPropertyNames);

            var readOnlyEvent = readOnlyEvents[0];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var readOnlyEventOldItems = readOnlyEvent.OldItems;

            Assert.That(readOnlyEventOldItems, Is.Not.Null);
            Assert.That(readOnlyEventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveRange_Event_OldItems));
            Assert.That(readOnlyEvent.OldStartingIndex, Is.EqualTo(expectedRemoveRange_Event_OldStartingIndex));

            var readOnlyEventNewItems = readOnlyEvent.NewItems;

            Assert.That(readOnlyEventNewItems, Is.Null);
        }



        [Test]
        public void CanRemoveWhere_NonEmptyObservableCollectionInReadOnlyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [0, 1, 2, 6, 7, 8, 9];
            var expectedItemsRemovedCount = 3;

            int[] expectedRemoveWhere_Event_OldItems = [3, 4, 5];
            var expectedRemoveWhere_Event_OldItemsCount = expectedRemoveWhere_Event_OldItems.Length;
            var expectedRemoveWhere_Event_OldStartingIndex = 3;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;
            var expectedReadOnlyEventCount = expectedEventCount;
            var expectedReadOnlyPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

            Assert.That(readOnlyCollection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var readOnlyEvents = readOnlyCollection.CollectionChangedEventArgsList;
            var readOnlyPropertyChangedEvents = readOnlyCollection.PropertyChangedEventArgsList;

            Assert.That(readOnlyEvents, Has.Count.EqualTo(0));
            Assert.That(readOnlyPropertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var itemsRemovedCount_RemoveWhere = collection.RemoveWhere(item => item is >= 3 and <= 5);

            Assert.That(itemsRemovedCount_RemoveWhere, Is.EqualTo(expectedItemsRemovedCount));

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
            Assert.That(readOnlyCollection, Is.EqualTo(expectedItems));
            Assert.That(readOnlyCollection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemoveWhere_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveWhere_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemoveWhere_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);

            Assert.That(readOnlyEvents, Has.Count.EqualTo(expectedReadOnlyEventCount));
            AssertPropertyChangedEvents(readOnlyPropertyChangedEvents, expectedReadOnlyPropertyChangedEventPropertyNames);

            var readOnlyEvent = readOnlyEvents[0];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var readOnlyEventOldItems = readOnlyEvent.OldItems;

            Assert.That(readOnlyEventOldItems, Is.Not.Null);
            Assert.That(readOnlyEventOldItems, Has.Count.EqualTo(expectedRemoveWhere_Event_OldItemsCount));
            Assert.That(readOnlyEventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveWhere_Event_OldItems));
            Assert.That(readOnlyEvent.OldStartingIndex, Is.EqualTo(expectedRemoveWhere_Event_OldStartingIndex));

            var readOnlyEventNewItems = readOnlyEvent.NewItems;

            Assert.That(readOnlyEventNewItems, Is.Null);
        }



        [Test]
        public void CanReplaceRange_NonEmptyObservableCollectionInReadOnlyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsReplacedIndex = 2;
            var itemsReplacedCount = 6;
            var itemsReplacement = items.GetRange(0..6);
            var itemsReplacementCount = itemsReplacement.Count;

            var expectedItems = items.Take(itemsReplacedIndex)
                                     .Concat(itemsReplacement)
                                     .Concat(items.Skip(itemsReplacedIndex + itemsReplacedCount))
                                     .ToArray();

            int[] expectedReplaceRange_Event_OldItems = [2, 3, 4, 5, 6, 7];
            var expectedReplaceRange_Event_OldStartingIndex = itemsReplacedIndex;
            int[] expectedReplaceRange_Event_NewItems = [0, 1, 2, 3, 4, 5];
            var expectedReplaceRange_Event_NewStartingIndex = itemsReplacedIndex;
            var expectedEventCount = 1;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;
            var expectedReadOnlyEventCount = expectedEventCount;
            var expectedReadOnlyPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

            Assert.That(readOnlyCollection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var readOnlyEvents = readOnlyCollection.CollectionChangedEventArgsList;
            var readOnlyPropertyChangedEvents = readOnlyCollection.PropertyChangedEventArgsList;

            Assert.That(readOnlyEvents, Has.Count.EqualTo(0));
            Assert.That(readOnlyPropertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ReplaceRange
            //
            collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement);

            collectionCount = collectionCount - itemsReplacedCount + itemsReplacementCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
            Assert.That(readOnlyCollection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            Assert.That(readOnlyEvents, Has.Count.EqualTo(expectedReadOnlyEventCount));
            AssertPropertyChangedEvents(readOnlyPropertyChangedEvents, expectedReadOnlyPropertyChangedEventPropertyNames);

            var readOnlyEvent = readOnlyEvents[0];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            var readOnlyEventOldItems = readOnlyEvent.OldItems;

            Assert.That(readOnlyEventOldItems, Is.Not.Null);
            Assert.That(readOnlyEventOldItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_OldItems));
            Assert.That(readOnlyEvent.OldStartingIndex, Is.EqualTo(expectedReplaceRange_Event_OldStartingIndex));

            var readOnlyEventNewItems = readOnlyEvent.NewItems;

            Assert.That(readOnlyEventNewItems, Is.Not.Null);
            Assert.That(readOnlyEventNewItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_NewItems));
            Assert.That(readOnlyEvent.NewStartingIndex, Is.EqualTo(expectedReplaceRange_Event_NewStartingIndex));
        }

        [Test]
        public void CanReplaceRange_WithAddRange_NonEmptyObservableCollectionInReadOnlyObservableCollection()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsReplacedIndex = 4;
            var itemsReplacedCount = 4;
            var itemsReplacement = items.GetRange(0..);
            var itemsReplacementCount = itemsReplacement.Count;

            var expectedItems = items.Take(itemsReplacedIndex)
                                     .Concat(itemsReplacement)
                                     .Concat(items.Skip(itemsReplacedIndex + itemsReplacedCount))
                                     .ToArray();

            int[] expectedReplaceRange_Event_OldItems = [4, 5, 6, 7, 8, 9];
            var expectedReplaceRange_Event_OldStartingIndex = itemsReplacedIndex;
            int[] expectedReplaceRange_Event_NewItems = [0, 1, 2, 3, 4, 5];
            var expectedReplaceRange_Event_NewStartingIndex = itemsReplacedIndex;
            int[] expectedAddRange_Event_NewItems = [6, 7, 8, 9, 8, 9];
            var expectedAddRange_Event_NewStartingIndex = itemsCount;
            var expectedEventCount = 2;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;
            var expectedReadOnlyEventCount = expectedEventCount;
            var expectedReadOnlyPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

            Assert.That(readOnlyCollection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var readOnlyEvents = readOnlyCollection.CollectionChangedEventArgsList;
            var readOnlyPropertyChangedEvents = readOnlyCollection.PropertyChangedEventArgsList;

            Assert.That(readOnlyEvents, Has.Count.EqualTo(0));
            Assert.That(readOnlyPropertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ReplaceRange
            //
            collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement);

            collectionCount = collectionCount - itemsReplacedCount + itemsReplacementCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
            Assert.That(readOnlyCollection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            Assert.That(readOnlyEvents, Has.Count.EqualTo(expectedReadOnlyEventCount));
            AssertPropertyChangedEvents(readOnlyPropertyChangedEvents, expectedReadOnlyPropertyChangedEventPropertyNames);

            NotifyCollectionChangedEventArgs readOnlyEvent;
            IList? readOnlyEventOldItems;
            IList? readOnlyEventNewItems;

            //
            // ReplaceRange Events
            //
            readOnlyEvent = readOnlyEvents[0];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            readOnlyEventOldItems = readOnlyEvent.OldItems;

            Assert.That(readOnlyEventOldItems, Is.Not.Null);
            Assert.That(readOnlyEventOldItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_OldItems));
            Assert.That(readOnlyEvent.OldStartingIndex, Is.EqualTo(expectedReplaceRange_Event_OldStartingIndex));

            readOnlyEventNewItems = readOnlyEvent.NewItems;

            Assert.That(readOnlyEventNewItems, Is.Not.Null);
            Assert.That(readOnlyEventNewItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_NewItems));
            Assert.That(readOnlyEvent.NewStartingIndex, Is.EqualTo(expectedReplaceRange_Event_NewStartingIndex));

            //
            // AddRange Events
            //
            readOnlyEvent = readOnlyEvents[expectedEventCount - 1];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            readOnlyEventOldItems = readOnlyEvent.OldItems;

            Assert.That(readOnlyEventOldItems, Is.Null);

            readOnlyEventNewItems = readOnlyEvent.NewItems;

            Assert.That(readOnlyEventNewItems, Is.Not.Null);
            Assert.That(readOnlyEventNewItems!.Cast<int>(), Is.EqualTo(expectedAddRange_Event_NewItems));
            Assert.That(readOnlyEvent.NewStartingIndex, Is.EqualTo(expectedAddRange_Event_NewStartingIndex));
        }

        [Test]
        public void CanReplaceRange_WithRemoveRange_NonEmptyObservableCollectionInReadOnlyObservableCollection()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemsReplacedIndex = 4;
            var itemsReplacedCount = 6;
            var itemsReplacement = items.GetRange(0..2);
            var itemsReplacementCount = itemsReplacement.Count;

            var expectedItems = items.Take(itemsReplacedIndex)
                                     .Concat(itemsReplacement)
                                     .Concat(items.Skip(itemsReplacedIndex + itemsReplacedCount))
                                     .ToArray();

            int[] expectedReplaceRange_Event_OldItems = [4, 5];
            var expectedReplaceRange_Event_OldStartingIndex = itemsReplacedIndex;
            int[] expectedReplaceRange_Event_NewItems = [0, 1];
            var expectedReplaceRange_Event_NewStartingIndex = itemsReplacedIndex;
            int[] expectedRemoveRange_Event_OldItems = [6, 7, 8, 9];
            var expectedRemoveRange_Event_OldStartingIndex = expectedItems.Length;
            var expectedEventCount = 2;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;
            var expectedReadOnlyEventCount = expectedEventCount;
            var expectedReadOnlyPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableCollection<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var readOnlyCollection = new TestReadOnlyObservableCollection<int>(collection);

            Assert.That(readOnlyCollection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var readOnlyEvents = readOnlyCollection.CollectionChangedEventArgsList;
            var readOnlyPropertyChangedEvents = readOnlyCollection.PropertyChangedEventArgsList;

            Assert.That(readOnlyEvents, Has.Count.EqualTo(0));
            Assert.That(readOnlyPropertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ReplaceRange
            //
            collection.ReplaceRange(itemsReplacedIndex, itemsReplacedCount, itemsReplacement);

            collectionCount = collectionCount - itemsReplacedCount + itemsReplacementCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(readOnlyCollection, Has.Count.EqualTo(collectionCount));
            Assert.That(readOnlyCollection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            Assert.That(readOnlyEvents, Has.Count.EqualTo(expectedReadOnlyEventCount));
            AssertPropertyChangedEvents(readOnlyPropertyChangedEvents, expectedReadOnlyPropertyChangedEventPropertyNames);

            NotifyCollectionChangedEventArgs readOnlyEvent;
            IList? readOnlyEventOldItems;
            IList? readOnlyEventNewItems;

            //
            // ReplaceRange Events
            //
            readOnlyEvent = readOnlyEvents[0];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));

            readOnlyEventOldItems = readOnlyEvent.OldItems;

            Assert.That(readOnlyEventOldItems, Is.Not.Null);
            Assert.That(readOnlyEventOldItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_OldItems));
            Assert.That(readOnlyEvent.OldStartingIndex, Is.EqualTo(expectedReplaceRange_Event_OldStartingIndex));

            readOnlyEventNewItems = readOnlyEvent.NewItems;

            Assert.That(readOnlyEventNewItems, Is.Not.Null);
            Assert.That(readOnlyEventNewItems!.Cast<int>(), Is.EqualTo(expectedReplaceRange_Event_NewItems));
            Assert.That(readOnlyEvent.NewStartingIndex, Is.EqualTo(expectedReplaceRange_Event_NewStartingIndex));

            //
            // RemoveRange Events
            //
            readOnlyEvent = readOnlyEvents[expectedEventCount - 1];

            Assert.That(readOnlyEvent, Is.Not.Null);
            Assert.That(readOnlyEvent.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            readOnlyEventOldItems = readOnlyEvent.OldItems;

            Assert.That(readOnlyEventOldItems, Is.Not.Null);
            Assert.That(readOnlyEventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveRange_Event_OldItems));
            Assert.That(readOnlyEvent.OldStartingIndex, Is.EqualTo(expectedRemoveRange_Event_OldStartingIndex));

            readOnlyEventNewItems = readOnlyEvent.NewItems;

            Assert.That(readOnlyEventNewItems, Is.Null);
        }

        #endregion
    }
}
