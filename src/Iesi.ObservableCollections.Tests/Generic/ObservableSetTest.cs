namespace Iesi.Collections.Tests.Generic
{
    [TestFixture(EventNotificationOrderMode.Strict)]
    [TestFixture(EventNotificationOrderMode.Relaxed)]
    public class ObservableSetTest : TestBase
    {
        private readonly List<int> _items = [];

        #region Setup

        public ObservableSetTest(EventNotificationOrderMode eventNotificationOrderMode) :
            base(eventNotificationOrderMode)
        {
        }

        [OneTimeSetUp]
        public void SetupFixture()
        {
            _items.Clear();
            _items.AddRange(Enumerable.Range(0, 10));
        }

        #endregion



        #region Constructor

        [Test]
        public void Constructor_PreservesInsertionOrder()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            //
            // Constructor
            //
            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void Constructor_RemovesDuplicatesAndPreservesFirstOccurrenceOrder()
        {
            List<int> items = [1, 2, 1, 3, 2, 4];
            //var itemsCount = items.Count;

            int[] expectedItems = [1, 2, 3, 4];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            //
            // Constructor
            //
            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void Constructor_RemovesDuplicatesUsingCustomComparerAndPreservesFirstOccurrenceOrder()
        {
            IEqualityComparer<string> itemEqualityComparer = StringComparer.OrdinalIgnoreCase;

            List<string> items = ["ABC", "abc", "DEF", "def", "GHI"];

            string[] expectedItems = ["ABC", "DEF", "GHI"];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            //
            // Constructor
            //
            var collection = new TestObservableSet<string>(items, itemEqualityComparer);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region Refresh

        [Test]
        public void CanRefresh_EmptyObservableSet()
        {
            //var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [];

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>();
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Refresh
            //
            collection.Refresh();

            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        [Test]
        public void CanRefresh_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Refresh
            //
            collection.Refresh();

            Assert.That(collection, Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        #endregion



        #region IndexOf

        [Test]
        public void CanIndexOf_UsingCustomComparer()
        {
            IEqualityComparer<string> itemEqualityComparer = StringComparer.OrdinalIgnoreCase;

            var item = "ABC";
            List<string> items = [item];

            var itemSearched = "abc";

            var expectedItemIndex = 0;

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<string>(items, itemEqualityComparer);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            Assert.That(collection.Contains(itemSearched));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // IndexOf
            //
            var itemIndex = collection.IndexOf(itemSearched);

            Assert.That(itemIndex, Is.EqualTo(expectedItemIndex));

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region Add / Insert

        [Test]
        public void CanAdd_EmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAdded = items[0];
            var itemsAddedCount = 1;

            int[] expectedItems = [itemAdded];

            int[] expectedEvent_Add_NewItems = [itemAdded];
            var expectedEvent_Add_NewItems_Count = expectedEvent_Add_NewItems.Length;
            var expectedEvent_Add_NewStartingIndex = 0;
            var expectedEventsCount = itemsAddedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Add
            //
            var isAdded = collection.Add(itemAdded);

            Assert.That(isAdded);

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Add_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Add_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Add_NewStartingIndex));
        }

        [Test]
        public void CanAdd_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemAdded = items[^1] + 1;
            var itemsAddedCount = 1;

            int[] expectedItems = [.. items, itemAdded];

            int[] expectedEvent_Add_NewItems = [itemAdded];
            var expectedEvent_Add_NewItems_Count = expectedEvent_Add_NewItems.Length;
            var expectedEvent_Add_NewStartingIndex = itemsCount;
            var expectedEventsCount = itemsAddedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Add
            //
            var isAdded = collection.Add(itemAdded);

            Assert.That(isAdded);

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(@event.OldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Add_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Add_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Add_NewStartingIndex));
        }

        [Test]
        public void CannotAdd_DuplicateItem_NonEmptyObservableSet()
        {
            int[] items = [_items[0]];
            //var itemsCount = items.Count;

            var itemAdded = items[0];
            var itemsAddedCount = 0;

            int[] expectedItems = [itemAdded];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Add
            //
            var isAdded = collection.Add(itemAdded);

            Assert.That(isAdded, Is.False);

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotAdd_DuplicateItem_UsingCustomComparer_NonEmptyObservableSet()
        {
            IEqualityComparer<string> itemEqualityComparer = StringComparer.OrdinalIgnoreCase;

            var item = "ABC";
            List<string> items = [item];

            var itemAdded = "abc";
            var itemsAddedCount = 0;

            string[] expectedItems = [item];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<string>(items, itemEqualityComparer);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            Assert.That(collection.Contains(itemAdded));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Add
            //
            var isAdded = collection.Add(itemAdded);

            Assert.That(isAdded, Is.False);

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }



        [Test]
        public void CanInsert_EmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = 0;
            var itemInserted = items[0];
            var itemsInsertedCount = 1;

            int[] expectedItems = [itemInserted];

            int[] expectedEvent_Insert_NewItems = [itemInserted];
            var expectedEvent_Insert_NewItems_Count = expectedEvent_Insert_NewItems.Length;
            var expectedEvent_Insert_NewStartingIndex = itemInsertedIndex;
            var expectedEventsCount = itemsInsertedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Insert
            //
            var isInserted = collection.Insert(itemInsertedIndex, itemInserted);

            Assert.That(isInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Insert_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Insert_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Insert_NewStartingIndex));
        }

        [Test]
        public void CanInsert_FirstItem_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = 0;
            var itemInserted = items[^1] + 1;
            var itemsInsertedCount = 1;

            int[] expectedItems = [itemInserted, .. items];

            int[] expectedEvent_Insert_NewItems = [itemInserted];
            var expectedEvent_Insert_NewItems_Count = expectedEvent_Insert_NewItems.Length;
            var expectedEvent_Insert_NewStartingIndex = itemInsertedIndex;
            var expectedEventsCount = itemsInsertedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Insert
            //
            var isInserted = collection.Insert(itemInsertedIndex, itemInserted);

            Assert.That(isInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Insert_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Insert_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Insert_NewStartingIndex));
        }

        [Test]
        public void CanInsert_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = 3;
            var itemInserted = items[^1] + 1;
            var itemsInsertedCount = 1;

            var expectedItems = items.Take(itemInsertedIndex)
                                     .Concat([itemInserted])
                                     .Concat(items.Skip(itemInsertedIndex))
                                     .ToArray();

            int[] expectedEvent_Insert_NewItems = [itemInserted];
            var expectedEvent_Insert_NewItems_Count = expectedEvent_Insert_NewItems.Length;
            var expectedEvent_Insert_NewStartingIndex = itemInsertedIndex;
            var expectedEventsCount = itemsInsertedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Insert
            //
            var isInserted = collection.Insert(itemInsertedIndex, itemInserted);

            Assert.That(isInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Insert_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Insert_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Insert_NewStartingIndex));
        }

        [Test]
        public void CanInsert_LastItem_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemInsertedIndex = itemsCount;
            var itemInserted = items[^1] + 1;
            var itemsInsertedCount = 1;

            int[] expectedItems = [.. items, itemInserted];

            int[] expectedEvent_Insert_NewItems = [itemInserted];
            var expectedEvent_Insert_NewItems_Count = expectedEvent_Insert_NewItems.Length;
            var expectedEvent_Insert_NewStartingIndex = itemInsertedIndex;
            var expectedEventsCount = itemsInsertedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Insert
            //
            var isInserted = collection.Insert(itemInsertedIndex, itemInserted);

            Assert.That(isInserted);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Insert_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Insert_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Insert_NewStartingIndex));
        }

        [Test]
        public void CannotInsert_DuplicateItem_NonEmptyObservableSet()
        {
            int[] items = [_items[0]];
            //var itemsCount = items.Length;

            var itemInsertedIndex = 0;
            var itemInserted = items[0];
            var itemsInsertedCount = 0;

            int[] expectedItems = [itemInserted];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Insert
            //
            var isInserted = collection.Insert(itemInsertedIndex, itemInserted);

            Assert.That(isInserted, Is.False);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotInsert_DuplicateItem_UsingCustomComparer_NonEmptyObservableSet()
        {
            IEqualityComparer<string> itemEqualityComparer = StringComparer.OrdinalIgnoreCase;

            var item = "ABC";
            List<string> items = [item];

            var itemInsertedIndex = 0;
            var itemInserted = "abc";
            var itemsInsertedCount = 0;

            string[] expectedItems = [item];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<string>(items, itemEqualityComparer);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            Assert.That(collection.Contains(itemInserted));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Insert
            //
            var isInserted = collection.Insert(itemInsertedIndex, itemInserted);

            Assert.That(isInserted, Is.False);

            collectionCount += itemsInsertedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotInsert_WithIndexLessThanZero_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = -1;
            var itemInserted = items[^1] + 1;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "index";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Insert
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Insert(itemInsertedIndex, itemInserted));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotInsert_WithIndexGreaterThanCount_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemInsertedIndex = itemsCount + 1;
            var itemInserted = items[^1] + 1;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "index";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Insert
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Insert(itemInsertedIndex, itemInserted));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region Remove / RemoveAt

        [Test]
        public void CanRemove_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            int[] expectedEvent_Remove_OldItems = [itemRemoved];
            var expectedEvent_Remove_OldItems_Count = expectedEvent_Remove_OldItems.Length;
            var expectedEvent_Remove_OldStartingIndex = itemRemovedIndex;
            var expectedEventsCount = itemsRemovedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var isRemoved = collection.Remove(itemRemoved);

            Assert.That(isRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Remove_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_Remove_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Remove_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanRemove_FirstItem_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 0;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            int[] expectedEvent_Remove_OldItems = [itemRemoved];
            var expectedEvent_Remove_OldItems_Count = expectedEvent_Remove_OldItems.Length;
            var expectedEvent_Remove_OldStartingIndex = itemRemovedIndex;
            var expectedEventsCount = itemsRemovedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Remove
            //
            var isRemoved = collection.Remove(itemRemoved);

            Assert.That(isRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Remove_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_Remove_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Remove_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanRemove_LastItem_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemRemovedIndex = itemsCount - 1;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            int[] expectedEvent_Remove_OldItems = [itemRemoved];
            var expectedEvent_Remove_OldItems_Count = expectedEvent_Remove_OldItems.Length;
            var expectedEvent_Remove_OldStartingIndex = itemRemovedIndex;
            var expectedEventsCount = itemsRemovedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Remove
            //
            var isRemoved = collection.Remove(itemRemoved);

            Assert.That(isRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Remove_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_Remove_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Remove_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CannotRemove_MissingItem_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemoved = items[^1] + 1;
            var itemsRemovedCount = 0;

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var isRemoved = collection.Remove(itemRemoved);

            Assert.That(isRemoved, Is.False);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CanRemove_UsingCustomComparer_NonEmptyObservableSet()
        {
            IEqualityComparer<string> itemEqualityComparer = StringComparer.OrdinalIgnoreCase;

            var item = "ABC";
            List<string> items = [item];
            //var itemsCount = items.Count;

            var itemRemoved = "abc";
            var itemRemovedIndex = 0;
            var itemsRemovedCount = 1;

            //int[] expectedItems = [];

            string[] expectedEvent_Remove_OldItems = [item];
            var expectedEvent_Remove_OldItems_Count = expectedEvent_Remove_OldItems.Length;
            var expectedEvent_Remove_OldStartingIndex = itemRemovedIndex;
            var expectedEventsCount = itemsRemovedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<string>(items, itemEqualityComparer);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            Assert.That(collection.Contains(itemRemoved));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var isRemoved = collection.Remove(itemRemoved);

            Assert.That(isRemoved);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(collection.Contains(item), Is.False);
            Assert.That(collection.Contains(itemRemoved), Is.False);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Remove_OldItems_Count));
            Assert.That(eventOldItems!.Cast<string>(), Is.EqualTo(expectedEvent_Remove_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Remove_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }



        [Test]
        public void CanRemoveAt_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            int[] expectedEvent_RemoveAt_OldItems = [itemRemoved];
            var expectedEvent_RemoveAt_OldItems_Count = expectedEvent_RemoveAt_OldItems.Length;
            var expectedEvent_RemoveAt_OldStartingIndex = itemRemovedIndex;
            var expectedEventsCount = itemsRemovedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveAt
            //
            collection.RemoveAt(itemRemovedIndex);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_RemoveAt_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_RemoveAt_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_RemoveAt_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanRemoveAt_FirstItem_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 0;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            int[] expectedEvent_RemoveAt_OldItems = [itemRemoved];
            var expectedEvent_RemoveAt_OldItems_Count = expectedEvent_RemoveAt_OldItems.Length;
            var expectedEvent_RemoveAt_OldStartingIndex = itemRemovedIndex;
            var expectedEventsCount = itemsRemovedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveAt
            //
            collection.RemoveAt(itemRemovedIndex);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_RemoveAt_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_RemoveAt_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_RemoveAt_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanRemoveAt_LastItem_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemRemovedIndex = itemsCount - 1;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            int[] expectedEvent_RemoveAt_OldItems = [itemRemoved];
            var expectedEvent_RemoveAt_OldItems_Count = expectedEvent_RemoveAt_OldItems.Length;
            var expectedEvent_RemoveAt_OldStartingIndex = itemRemovedIndex;
            var expectedEventsCount = itemsRemovedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveAt
            //
            collection.RemoveAt(itemRemovedIndex);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_RemoveAt_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_RemoveAt_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_RemoveAt_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CannotRemoveAt_WithIndexLessThanZero_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = -1;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "index";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveAt
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.RemoveAt(itemRemovedIndex));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotRemoveAt_WithIndexEqualToCount_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemRemovedIndex = itemsCount;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "index";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveAt
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.RemoveAt(itemRemovedIndex));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotRemoveAt_EmptyObservableSet()
        {
            var itemRemovedIndex = 0;

            var expectedExceptionParameterName = "index";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>();
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveAt
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.RemoveAt(itemRemovedIndex));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region Move

        [Test]
        public void CanMove_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemMovedIndexOld = 3;
            var itemMovedIndexNew = 6;
            var itemMoved = items[itemMovedIndexOld];

            var expectedItems = items.ToList();

            expectedItems.RemoveAt(itemMovedIndexOld);
            expectedItems.Insert(itemMovedIndexNew, itemMoved);

            int[] expectedEvent_Move_NewItems = [itemMoved];
            var expectedEvent_Move_NewItems_Count = expectedEvent_Move_NewItems.Length;
            var expectedEvent_Move_NewStartingIndex = itemMovedIndexNew;

            int[] expectedEvent_Move_OldItems = [itemMoved];
            var expectedEvent_Move_OldItems_Count = expectedEvent_Move_OldItems.Length;
            var expectedEvent_Move_OldStartingIndex = itemMovedIndexOld;

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            collection.Move(itemMovedIndexOld, itemMovedIndexNew);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Move));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Move_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_Move_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Move_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Move_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Move_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Move_NewStartingIndex));
        }

        [Test]
        public void CanMove_FirstItemToLastIndex_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemMovedIndexOld = 0;
            var itemMovedIndexNew = itemsCount - 1;
            var itemMoved = items[itemMovedIndexOld];

            var expectedItems = items.ToList();

            expectedItems.RemoveAt(itemMovedIndexOld);
            expectedItems.Insert(itemMovedIndexNew, itemMoved);

            int[] expectedEvent_Move_NewItems = [itemMoved];
            var expectedEvent_Move_NewItems_Count = expectedEvent_Move_NewItems.Length;
            var expectedEvent_Move_NewStartingIndex = itemMovedIndexNew;

            int[] expectedEvent_Move_OldItems = [itemMoved];
            var expectedEvent_Move_OldItems_Count = expectedEvent_Move_OldItems.Length;
            var expectedEvent_Move_OldStartingIndex = itemMovedIndexOld;

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            collection.Move(itemMovedIndexOld, itemMovedIndexNew);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Move));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Move_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_Move_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Move_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Move_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Move_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Move_NewStartingIndex));
        }

        [Test]
        public void CanMove_LastItemToFirstIndex_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemMovedIndexOld = itemsCount - 1;
            var itemMovedIndexNew = 0;
            var itemMoved = items[itemMovedIndexOld];

            var expectedItems = items.ToList();

            expectedItems.RemoveAt(itemMovedIndexOld);
            expectedItems.Insert(itemMovedIndexNew, itemMoved);

            int[] expectedEvent_Move_NewItems = [itemMoved];
            var expectedEvent_Move_NewItems_Count = expectedEvent_Move_NewItems.Length;
            var expectedEvent_Move_NewStartingIndex = itemMovedIndexNew;

            int[] expectedEvent_Move_OldItems = [itemMoved];
            var expectedEvent_Move_OldItems_Count = expectedEvent_Move_OldItems.Length;
            var expectedEvent_Move_OldStartingIndex = itemMovedIndexOld;

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            collection.Move(itemMovedIndexOld, itemMovedIndexNew);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Move));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Move_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_Move_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Move_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Move_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Move_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Move_NewStartingIndex));
        }

        [Test]
        public void CanMove_ToSameIndex_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemMovedIndexOld = 3;
            var itemMovedIndexNew = itemMovedIndexOld;
            var itemMoved = items[itemMovedIndexOld];

            int[] expectedItems = [.. items];

            int[] expectedEvent_Move_NewItems = [itemMoved];
            var expectedEvent_Move_NewItems_Count = expectedEvent_Move_NewItems.Length;
            var expectedEvent_Move_NewStartingIndex = itemMovedIndexNew;

            int[] expectedEvent_Move_OldItems = [itemMoved];
            var expectedEvent_Move_OldItems_Count = expectedEvent_Move_OldItems.Length;
            var expectedEvent_Move_OldStartingIndex = itemMovedIndexOld;

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            collection.Move(itemMovedIndexOld, itemMovedIndexNew);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Move));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Move_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_Move_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Move_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Move_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Move_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Move_NewStartingIndex));
        }

        [Test]
        public void CannotMove_WithOldIndexLessThanZero_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemMovedIndexOld = -1;
            var itemMovedIndexNew = 3;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "oldIndex";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotMove_WithOldIndexEqualToCount_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemMovedIndexOld = itemsCount;
            var itemMovedIndexNew = 3;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "oldIndex";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotMove_WithNewIndexLessThanZero_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemMovedIndexOld = 3;
            var itemMovedIndexNew = -1;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "newIndex";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotMove_WithNewIndexEqualToCount_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemMovedIndexOld = 3;
            var itemMovedIndexNew = itemsCount;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "newIndex";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotMove_EmptyObservableSet()
        {
            var itemMovedIndexOld = 0;
            var itemMovedIndexNew = 0;

            var expectedExceptionParameterName = "oldIndex";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>();
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region Clear

        [Test]
        public void CanClear_EmptyObservableSet()
        {
            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>();
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // Clear
            //
            collection.Clear();

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CanClear_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsRemoved = [.. items];
            var itemsRemovedCount = itemsRemoved.Length;

            //int[] expectedItems = [];

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            collection.Clear();

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        #endregion



        #region RemoveWhere

        [Test]
        public void CanRemoveWhere_WithSingleCluster_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [0, 1, 2, 6, 7, 8, 9];
            var expectedItemsRemovedCount = 3;

            int[] expectedEvent_Remove_OldItems = [3, 4, 5];
            var expectedEvent_Remove_OldItems_Count = expectedEvent_Remove_OldItems.Length;
            var expectedEvent_Remove_OldStartingIndex = 3;
            var expectedEventsCount = 1;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
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

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Remove_OldItems_Count));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedEvent_Remove_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Remove_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CanRemoveWhere_WithMultipleClusters_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [0, 3, 4, 7, 8, 9];
            var expectedItemsRemovedCount = 4;

            int[] expectedEvent_0_Remove_OldItems = [1, 2];
            var expectedEvent_0_Remove_OldItems_Count = expectedEvent_0_Remove_OldItems.Length;
            var expectedEvent_0_Remove_OldStartingIndex = 1;

            int[] expectedEvent_1_Remove_OldItems = [5, 6];
            var expectedEvent_1_Remove_OldItems_Count = expectedEvent_1_Remove_OldItems.Length;
            var expectedEvent_1_Remove_OldStartingIndex = 3;

            var expectedEventsCount = 2;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
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

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var event_0 = events[0];

            Assert.That(event_0, Is.Not.Null);
            Assert.That(event_0.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var event_0_OldItems = event_0.OldItems;

            Assert.That(event_0_OldItems, Is.Not.Null);
            Assert.That(event_0_OldItems, Has.Count.EqualTo(expectedEvent_0_Remove_OldItems_Count));
            Assert.That(event_0_OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_0_Remove_OldItems));
            Assert.That(event_0.OldStartingIndex, Is.EqualTo(expectedEvent_0_Remove_OldStartingIndex));
            Assert.That(event_0.NewItems, Is.Null);

            var event_1 = events[1];

            Assert.That(event_1, Is.Not.Null);
            Assert.That(event_1.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var event_1_OldItems = event_1.OldItems;

            Assert.That(event_1_OldItems, Is.Not.Null);
            Assert.That(event_1_OldItems, Has.Count.EqualTo(expectedEvent_1_Remove_OldItems_Count));
            Assert.That(event_1_OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_1_Remove_OldItems));
            Assert.That(event_1.OldStartingIndex, Is.EqualTo(expectedEvent_1_Remove_OldStartingIndex));
            Assert.That(event_1.NewItems, Is.Null);
        }

        [Test]
        public void CanRemoveWhere_All_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            //int[] expectedItems = [];
            var expectedItemsRemovedCount = itemsCount;

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
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

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        [Test]
        public void CannotRemoveWhere_WithNoMatch_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];
            var expectedItemsRemovedCount = 0;

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
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

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotRemoveWhere_EmptyObservableSet()
        {
            //int[] expectedItems = [];
            var expectedItemsRemovedCount = 0;

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
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

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotRemoveWhere_WithNullMatch_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            Predicate<int>? match = null;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "match";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => collection.RemoveWhere(match!));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CanRemoveWhere_UsingCustomComparer_NonEmptyObservableSet()
        {
            IEqualityComparer<string> itemEqualityComparer = StringComparer.OrdinalIgnoreCase;

            var itemStored = "ABC";
            var itemRemoved = "abc";

            List<string> items = [itemStored, "DEF", "GHI"];
            //var itemsCount = items.Count;

            string[] expectedItems = ["DEF", "GHI"];
            var expectedItemsRemovedCount = 1;

            string[] expectedEvent_Remove_OldItems = [itemStored];
            var expectedEvent_Remove_OldItems_Count = expectedEvent_Remove_OldItems.Length;
            var expectedEvent_Remove_OldStartingIndex = 0;

            var expectedEventsCount = expectedItemsRemovedCount;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<string>(items, itemEqualityComparer);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            Assert.That(collection.Contains(itemRemoved));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // RemoveWhere
            //
            var itemsRemovedCount_RemoveWhere = collection.RemoveWhere(item => itemEqualityComparer.Equals(item, itemRemoved));

            Assert.That(itemsRemovedCount_RemoveWhere, Is.EqualTo(expectedItemsRemovedCount));

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemStored), Is.False);
            Assert.That(collection.Contains(itemRemoved), Is.False);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedEvent_Remove_OldItems_Count));
            Assert.That(eventOldItems!.Cast<string>(), Is.EqualTo(expectedEvent_Remove_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedEvent_Remove_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CannotRemoveWhere_WhenMatchThrows_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
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

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(2), Is.True);
            Assert.That(collection.Contains(3), Is.True);
            Assert.That(collection.Contains(4), Is.True);
            Assert.That(collection.Contains(5), Is.True);
            Assert.That(collection.Contains(6), Is.True);
            Assert.That(collection.Contains(7), Is.True);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region Set Algebra

        #region UnionWith

        [Test]
        public void CanUnionWith_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            int[] itemsOther = [5, 6, 10, 11, 10, 12];

            int[] expectedItems = [.. items, 10, 11, 12];

            int[] expectedEvent_Add_NewItems = [10, 11, 12];
            var expectedEvent_Add_NewItems_Count = expectedEvent_Add_NewItems.Length;
            var expectedEvent_Add_NewStartingIndex = itemsCount;

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // UnionWith
            //
            collection.UnionWith(itemsOther);

            collectionCount += expectedEvent_Add_NewItems_Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(@event.OldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Add_NewItems_Count));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedEvent_Add_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Add_NewStartingIndex));
        }

        [Test]
        public void CannotUnionWith_WithNoNewItems_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [0, 1, 2, 3, 3, 2, 1];

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // UnionWith
            //
            collection.UnionWith(itemsOther);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CanUnionWith_UsingCustomComparer_NonEmptyObservableSet()
        {
            IEqualityComparer<string> itemEqualityComparer = StringComparer.OrdinalIgnoreCase;

            List<string> items = ["ABC"];

            string[] itemsOther = ["abc", "DEF", "def", "GHI"];

            string[] expectedItems = ["ABC", "DEF", "GHI"];

            string[] expectedEvent_Add_NewItems = ["DEF", "GHI"];
            var expectedEvent_Add_NewItems_Count = expectedEvent_Add_NewItems.Length;
            var expectedEvent_Add_NewStartingIndex = 1;

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<string>(items, itemEqualityComparer);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // UnionWith
            //
            collection.UnionWith(itemsOther);

            collectionCount += expectedEvent_Add_NewItems_Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(@event.OldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedEvent_Add_NewItems_Count));
            Assert.That(eventNewItems!.Cast<string>(), Is.EqualTo(expectedEvent_Add_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedEvent_Add_NewStartingIndex));
        }

        [Test]
        public void CannotUnionWith_WithNullOther_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            IEnumerable<int>? itemsOther = null;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "other";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // UnionWith
            //
            var exception = Assert.Throws<ArgumentNullException>(
                () => collection.UnionWith(itemsOther!));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotUnionWith_WhenOtherEnumerationThrows_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemOther_0_Added = 10;
            var itemOther_1_Added = 11;

            var itemsOther =
                CreateThrowingEnumerable(
                    [itemOther_0_Added, itemOther_1_Added, 12],
                    2);

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // UnionWith
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.UnionWith(itemsOther));

            Assert.That(exception, Is.Not.Null);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemOther_0_Added), Is.False);
            Assert.That(collection.Contains(itemOther_1_Added), Is.False);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region IntersectWith

        [Test]
        public void CanIntersectWith_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [1, 2, 5, 6, 100];

            int[] expectedItems = [1, 2, 5, 6];
            var expectedItemsRemovedCount = 6;

            int[] expectedEvent_0_Remove_OldItems = [0];
            var expectedEvent_0_Remove_OldStartingIndex = 0;

            int[] expectedEvent_1_Remove_OldItems = [3, 4];
            var expectedEvent_1_Remove_OldStartingIndex = 2;

            int[] expectedEvent_2_Remove_OldItems = [7, 8, 9];
            var expectedEvent_2_Remove_OldStartingIndex = 4;

            var expectedEventsCount = 3;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // IntersectWith
            //
            collection.IntersectWith(itemsOther);

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var event_0 = events[0];

            Assert.That(event_0, Is.Not.Null);
            Assert.That(event_0.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(event_0.OldItems, Is.Not.Null);
            Assert.That(event_0.OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_0_Remove_OldItems));
            Assert.That(event_0.OldStartingIndex, Is.EqualTo(expectedEvent_0_Remove_OldStartingIndex));
            Assert.That(event_0.NewItems, Is.Null);

            var event_1 = events[1];

            Assert.That(event_1, Is.Not.Null);
            Assert.That(event_1.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(event_1.OldItems, Is.Not.Null);
            Assert.That(event_1.OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_1_Remove_OldItems));
            Assert.That(event_1.OldStartingIndex, Is.EqualTo(expectedEvent_1_Remove_OldStartingIndex));
            Assert.That(event_1.NewItems, Is.Null);

            var event_2 = events[2];

            Assert.That(event_2, Is.Not.Null);
            Assert.That(event_2.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(event_2.OldItems, Is.Not.Null);
            Assert.That(event_2.OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_2_Remove_OldItems));
            Assert.That(event_2.OldStartingIndex, Is.EqualTo(expectedEvent_2_Remove_OldStartingIndex));
            Assert.That(event_2.NewItems, Is.Null);
        }

        [Test]
        public void CanIntersectWith_WithEmptyOther_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [];

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // IntersectWith
            //
            collection.IntersectWith(itemsOther);

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        [Test]
        public void CannotIntersectWith_WithNoChange_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [.. items, 100, 101];

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // IntersectWith
            //
            collection.IntersectWith(itemsOther);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotIntersectWith_WithNullOther_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            IEnumerable<int>? itemsOther = null;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "other";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // IntersectWith
            //
            var exception = Assert.Throws<ArgumentNullException>(
                () => collection.IntersectWith(itemsOther!));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotIntersectWith_WhenOtherEnumerationThrows_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemOther_0_Kept = 1;
            var itemOther_1_Kept = 2;

            var itemsOther =
                CreateThrowingEnumerable(
                    [itemOther_0_Kept, itemOther_1_Kept, 5],
                    2);

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // IntersectWith
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.IntersectWith(itemsOther));

            Assert.That(exception, Is.Not.Null);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(0), Is.True);
            Assert.That(collection.Contains(3), Is.True);
            Assert.That(collection.Contains(4), Is.True);
            Assert.That(collection.Contains(7), Is.True);
            Assert.That(collection.Contains(8), Is.True);
            Assert.That(collection.Contains(9), Is.True);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region ExceptWith

        [Test]
        public void CanExceptWith_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [1, 2, 5, 6, 100];

            int[] expectedItems = [0, 3, 4, 7, 8, 9];
            var expectedItemsRemovedCount = 4;

            int[] expectedEvent_0_Remove_OldItems = [1, 2];
            var expectedEvent_0_Remove_OldStartingIndex = 1;

            int[] expectedEvent_1_Remove_OldItems = [5, 6];
            var expectedEvent_1_Remove_OldStartingIndex = 3;

            var expectedEventsCount = 2;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ExceptWith
            //
            collection.ExceptWith(itemsOther);

            collectionCount -= expectedItemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var event_0 = events[0];

            Assert.That(event_0, Is.Not.Null);
            Assert.That(event_0.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(event_0.OldItems, Is.Not.Null);
            Assert.That(event_0.OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_0_Remove_OldItems));
            Assert.That(event_0.OldStartingIndex, Is.EqualTo(expectedEvent_0_Remove_OldStartingIndex));
            Assert.That(event_0.NewItems, Is.Null);

            var event_1 = events[1];

            Assert.That(event_1, Is.Not.Null);
            Assert.That(event_1.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(event_1.OldItems, Is.Not.Null);
            Assert.That(event_1.OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_1_Remove_OldItems));
            Assert.That(event_1.OldStartingIndex, Is.EqualTo(expectedEvent_1_Remove_OldStartingIndex));
            Assert.That(event_1.NewItems, Is.Null);
        }

        [Test]
        public void CanExceptWith_All_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [.. items];

            var expectedEventsCount = 1;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ExceptWith
            //
            collection.ExceptWith(itemsOther);

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventsCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(@event.OldItems, Is.Null);
            Assert.That(@event.NewItems, Is.Null);
        }

        [Test]
        public void CannotExceptWith_WithNoMatch_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [100, 101, 102];

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ExceptWith
            //
            collection.ExceptWith(itemsOther);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotExceptWith_WithNullOther_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            IEnumerable<int>? itemsOther = null;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "other";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ExceptWith
            //
            var exception = Assert.Throws<ArgumentNullException>(
                () => collection.ExceptWith(itemsOther!));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotExceptWith_WhenOtherEnumerationThrows_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemOther_0_Removed = 1;
            var itemOther_1_Removed = 2;

            var itemsOther =
                CreateThrowingEnumerable(
                    [itemOther_0_Removed, itemOther_1_Removed, 5],
                    2);

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // ExceptWith
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.ExceptWith(itemsOther));

            Assert.That(exception, Is.Not.Null);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemOther_0_Removed), Is.True);
            Assert.That(collection.Contains(itemOther_1_Removed), Is.True);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region SymmetricExceptWith

        [Test]
        public void CanSymmetricExceptWith_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [2, 4, 10, 11, 10];

            int[] expectedItems = [0, 1, 3, 5, 6, 7, 8, 9, 10, 11];

            int[] expectedEvent_0_Remove_OldItems = [2];
            var expectedEvent_0_Remove_OldStartingIndex = 2;

            int[] expectedEvent_1_Remove_OldItems = [4];
            var expectedEvent_1_Remove_OldStartingIndex = 3;

            int[] expectedEvent_2_Add_NewItems = [10, 11];
            var expectedEvent_2_Add_NewStartingIndex = 8;

            var expectedEventsCount = 3;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // SymmetricExceptWith
            //
            collection.SymmetricExceptWith(itemsOther);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var event_0 = events[0];

            Assert.That(event_0, Is.Not.Null);
            Assert.That(event_0.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(event_0.OldItems, Is.Not.Null);
            Assert.That(event_0.OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_0_Remove_OldItems));
            Assert.That(event_0.OldStartingIndex, Is.EqualTo(expectedEvent_0_Remove_OldStartingIndex));
            Assert.That(event_0.NewItems, Is.Null);

            var event_1 = events[1];

            Assert.That(event_1, Is.Not.Null);
            Assert.That(event_1.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(event_1.OldItems, Is.Not.Null);
            Assert.That(event_1.OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_1_Remove_OldItems));
            Assert.That(event_1.OldStartingIndex, Is.EqualTo(expectedEvent_1_Remove_OldStartingIndex));
            Assert.That(event_1.NewItems, Is.Null);

            var event_2 = events[2];

            Assert.That(event_2, Is.Not.Null);
            Assert.That(event_2.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(event_2.OldItems, Is.Null);
            Assert.That(event_2.NewItems, Is.Not.Null);
            Assert.That(event_2.NewItems!.Cast<int>(), Is.EqualTo(expectedEvent_2_Add_NewItems));
            Assert.That(event_2.NewStartingIndex, Is.EqualTo(expectedEvent_2_Add_NewStartingIndex));
        }

        [Test]
        public void CanSymmetricExceptWith_WithCountChanged_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [2, 10, 11];

            int[] expectedItems = [0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11];

            int[] expectedEvent_0_Remove_OldItems = [2];
            var expectedEvent_0_Remove_OldStartingIndex = 2;

            int[] expectedEvent_1_Add_NewItems = [10, 11];
            var expectedEvent_1_Add_NewStartingIndex = 9;

            var expectedEventsCount = 2;
            var expectedPropertyChangingEventPropertyNames = CountPropertyChangingEventPropertyNames;
            var expectedPropertyChangedEventPropertyNames = CountAndIndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // SymmetricExceptWith
            //
            collection.SymmetricExceptWith(itemsOther);

            collectionCount++;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var event_0 = events[0];

            Assert.That(event_0, Is.Not.Null);
            Assert.That(event_0.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(event_0.OldItems, Is.Not.Null);
            Assert.That(event_0.OldItems!.Cast<int>(), Is.EqualTo(expectedEvent_0_Remove_OldItems));
            Assert.That(event_0.OldStartingIndex, Is.EqualTo(expectedEvent_0_Remove_OldStartingIndex));
            Assert.That(event_0.NewItems, Is.Null);

            var event_1 = events[1];

            Assert.That(event_1, Is.Not.Null);
            Assert.That(event_1.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(event_1.OldItems, Is.Null);
            Assert.That(event_1.NewItems, Is.Not.Null);
            Assert.That(event_1.NewItems!.Cast<int>(), Is.EqualTo(expectedEvent_1_Add_NewItems));
            Assert.That(event_1.NewStartingIndex, Is.EqualTo(expectedEvent_1_Add_NewStartingIndex));
        }

        [Test]
        public void CanSymmetricExceptWith_UsingCustomComparer_NonEmptyObservableSet()
        {
            IEqualityComparer<string> itemEqualityComparer = StringComparer.OrdinalIgnoreCase;

            List<string> items = ["ABC", "DEF"];

            string[] itemsOther = ["abc", "ghi", "GHI"];

            string[] expectedItems = ["DEF", "ghi"];

            string[] expectedEvent_0_Remove_OldItems = ["ABC"];
            var expectedEvent_0_Remove_OldStartingIndex = 0;

            string[] expectedEvent_1_Add_NewItems = ["ghi"];
            var expectedEvent_1_Add_NewStartingIndex = 1;

            var expectedEventsCount = 2;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

            var collection = new TestObservableSet<string>(items, itemEqualityComparer);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // SymmetricExceptWith
            //
            collection.SymmetricExceptWith(itemsOther);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var event_0 = events[0];

            Assert.That(event_0, Is.Not.Null);
            Assert.That(event_0.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(event_0.OldItems, Is.Not.Null);
            Assert.That(event_0.OldItems!.Cast<string>(), Is.EqualTo(expectedEvent_0_Remove_OldItems));
            Assert.That(event_0.OldStartingIndex, Is.EqualTo(expectedEvent_0_Remove_OldStartingIndex));
            Assert.That(event_0.NewItems, Is.Null);

            var event_1 = events[1];

            Assert.That(event_1, Is.Not.Null);
            Assert.That(event_1.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(event_1.OldItems, Is.Null);
            Assert.That(event_1.NewItems, Is.Not.Null);
            Assert.That(event_1.NewItems!.Cast<string>(), Is.EqualTo(expectedEvent_1_Add_NewItems));
            Assert.That(event_1.NewStartingIndex, Is.EqualTo(expectedEvent_1_Add_NewStartingIndex));
        }

        [Test]
        public void CannotSymmetricExceptWith_WithEmptyOther_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [];

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // SymmetricExceptWith
            //
            collection.SymmetricExceptWith(itemsOther);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotSymmetricExceptWith_WithNullOther_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            IEnumerable<int>? itemsOther = null;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "other";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // SymmetricExceptWith
            //
            var exception = Assert.Throws<ArgumentNullException>(
                () => collection.SymmetricExceptWith(itemsOther!));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotSymmetricExceptWith_WhenOtherEnumerationThrows_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemOther_0_Removed = 2;
            var itemOther_1_Added = 10;

            var itemsOther =
                CreateThrowingEnumerable(
                    [itemOther_0_Removed, itemOther_1_Added, 11],
                    2);

            int[] expectedItems = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(items));
            Assert.That(collection.ToArray(), Is.EqualTo(items));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // SymmetricExceptWith
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.SymmetricExceptWith(itemsOther));

            Assert.That(exception, Is.Not.Null);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemOther_0_Removed), Is.True);
            Assert.That(collection.Contains(itemOther_1_Added), Is.False);

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region Set Algebra Helper Methods

        private static IEnumerable<T> CreateThrowingEnumerable<T>(
            IEnumerable<T> items,
            int throwAfterItemsCount)
        {
            var itemsEnumeratedCount = 0;

            foreach (var item in items)
            {
                if (itemsEnumeratedCount == throwAfterItemsCount)
                {
                    throw new InvalidOperationException();
                }

                yield return item;

                itemsEnumeratedCount++;
            }

            if (itemsEnumeratedCount == throwAfterItemsCount)
            {
                throw new InvalidOperationException();
            }
        }

        #endregion

        #endregion



        #region CopyTo

        [Test]
        public void CanCopyTo_PreservesInsertionOrder_NonEmptyObservableSet()
        {
            int[] items = [7, 3, 9, 1, 5, 2];
            //var itemsCount = items.Length;

            int[] expectedItems = [.. items];
            int[] expectedArrayDestination = [.. items];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var arrayDestination = new int[items.Length];

            //
            // CopyTo
            //
            collection.CopyTo(arrayDestination);

            Assert.That(arrayDestination, Is.EqualTo(expectedArrayDestination));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CanCopyTo_WithArrayIndex_PreservesInsertionOrder_NonEmptyObservableSet()
        {
            int[] items = [7, 3, 9, 1, 5, 2];
            //var itemsCount = items.Length;

            var arrayDestinationItemUntouched = -1;
            var arrayDestinationStartingIndex = 2;

            int[] expectedItems = [.. items];
            int[] expectedArrayDestination = [arrayDestinationItemUntouched, arrayDestinationItemUntouched, .. items, arrayDestinationItemUntouched];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var arrayDestination =
                Enumerable.Repeat(arrayDestinationItemUntouched, items.Length + arrayDestinationStartingIndex + 1)
                          .ToArray();

            //
            // CopyTo
            //
            collection.CopyTo(arrayDestination, arrayDestinationStartingIndex);

            Assert.That(arrayDestination, Is.EqualTo(expectedArrayDestination));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CanCopyTo_WithArrayIndexAndCount_PreservesInsertionOrder_NonEmptyObservableSet()
        {
            int[] items = [7, 3, 9, 1, 5, 2];
            //var itemsCount = items.Length;

            var arrayDestinationItemUntouched = -1;
            var arrayDestinationStartingIndex = 2;
            var arrayDestinationItemCount = 4;

            int[] expectedItems = [.. items];
            int[] expectedArrayDestination =
            [
                arrayDestinationItemUntouched,
                arrayDestinationItemUntouched,
                7,
                3,
                9,
                1,
                arrayDestinationItemUntouched
            ];

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var arrayDestination =
                Enumerable.Repeat(arrayDestinationItemUntouched, expectedArrayDestination.Length)
                          .ToArray();

            //
            // CopyTo
            //
            collection.CopyTo(arrayDestination, arrayDestinationStartingIndex, arrayDestinationItemCount);

            Assert.That(arrayDestination, Is.EqualTo(expectedArrayDestination));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotCopyTo_WithNullArray_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];

            int[]? arrayDestination = null;
            var arrayDestinationStartingIndex = 0;
            var arrayDestinationItemCount = items.Count;

            var expectedExceptionParameterName = "array";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            //
            // CopyTo
            //
            var exception = Assert.Throws<ArgumentNullException>(
                () => collection.CopyTo(arrayDestination!, arrayDestinationStartingIndex, arrayDestinationItemCount));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotCopyTo_WithArrayIndexLessThanZero_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "arrayIndex";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var arrayDestination = new int[items.Count];
            var arrayDestinationIndex = -1;
            var arrayDestinationCount = items.Count;

            //
            // CopyTo
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.CopyTo(arrayDestination, arrayDestinationIndex, arrayDestinationCount));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotCopyTo_WithCountGreaterThanCollectionCount_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "count";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var arrayDestination = new int[itemsCount + 1];
            var arrayDestinationIndex = 0;
            var arrayDestinationCount = itemsCount + 1;

            //
            // CopyTo
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.CopyTo(arrayDestination, arrayDestinationIndex, arrayDestinationCount));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        [Test]
        public void CannotCopyTo_WithInsufficientDestinationCapacity_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "array";

            var expectedEventsCount = 0;
            var expectedPropertyChangingEventsCount = 0;
            var expectedPropertyChangedEventsCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(0));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(0));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(0));

            var arrayDestination = new int[itemsCount + 1];
            var arrayDestinationIndex = 2;
            var arrayDestinationCount = itemsCount;

            //
            // CopyTo
            //
            var exception = Assert.Throws<ArgumentException>(
                () => collection.CopyTo(arrayDestination, arrayDestinationIndex, arrayDestinationCount));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventsCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventsCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventsCount));
        }

        #endregion



        #region Reentrancy

        [Test]
        public void CanAdd_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableSet()
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

            var collection = new TestObservableSet<int>(items);
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
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanAdd_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAdded = 100;
            var itemAddedReentrantly = 999;

            int[] expectedItems = [.. items, itemAdded];

            var collection = new TestObservableSet<int>(items);
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
            var isAdded = false;

            Assert.DoesNotThrow(
                () => isAdded = collection.Add(itemAdded));

            Assert.That(isAdded);

            collectionCount++;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemAdded), Is.True);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotAdd_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAdded = 100;

            var itemAddedReentrantly = 999;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

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
        public void CanInsert_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = 3;
            var itemInserted = 100;

            var itemAddedReentrantly = 999;
            var itemsInsertedCount = 1;
            var itemsAddedReentrantlyCount = 1;

            var expectedItems = items.Take(itemInsertedIndex)
                                     .Concat([itemInserted])
                                     .Concat(items.Skip(itemInsertedIndex))
                                     .Concat([itemAddedReentrantly])
                                     .ToArray();

            var collection = new TestObservableSet<int>(items);
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
            // Insert
            //
            var isInserted = false;

            Assert.DoesNotThrow(
                () => isInserted = collection.Insert(itemInsertedIndex, itemInserted));

            Assert.That(isInserted);

            collectionCount += itemsInsertedCount + itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemInserted), Is.True);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.True);
        }

        [Test]
        public void CanInsert_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
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

            var collection = new TestObservableSet<int>(items);
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
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemInserted), Is.True);
            Assert.That(collection.Contains(itemInsertedReentrantly), Is.False);
        }

        [Test]
        public void CannotInsert_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemInsertedIndex = 3;
            var itemInserted = 100;

            var itemInsertedReentrantlyIndex = 4;
            var itemInsertedReentrantly = 999;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

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
        public void CanRemove_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemAddedReentrantly = 999;
            var itemsRemovedCount = 1;
            var itemsAddedReentrantlyCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .Concat([itemAddedReentrantly])
                                     .ToArray();

            var collection = new TestObservableSet<int>(items);
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
            // Remove
            //
            var isRemoved = false;

            Assert.DoesNotThrow(
                () => isRemoved = collection.Remove(itemRemoved));

            Assert.That(isRemoved);

            collectionCount -= itemsRemovedCount;
            collectionCount += itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemRemoved), Is.False);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.True);
        }

        [Test]
        public void CanRemove_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemRemovedReentrantlyIndex = 4;
            var itemRemovedReentrantly = items[itemRemovedReentrantlyIndex];

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            var collection = new TestObservableSet<int>(items);
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
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemRemoved), Is.False);
            Assert.That(collection.Contains(itemRemovedReentrantly), Is.True);
        }

        [Test]
        public void CannotRemove_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemRemovedReentrantlyIndex = 4;
            var itemRemovedReentrantly = items[itemRemovedReentrantlyIndex];

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

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
        public void CanRemoveAt_ByIndex_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemAddedReentrantly = 999;
            var itemsRemovedCount = 1;
            var itemsAddedReentrantlyCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .Concat([itemAddedReentrantly])
                                     .ToArray();

            var collection = new TestObservableSet<int>(items);
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
            // RemoveAt (By Index)
            //
            Assert.DoesNotThrow(
                () => collection.RemoveAt(itemRemovedIndex));

            collectionCount -= itemsRemovedCount;
            collectionCount += itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemRemoved), Is.False);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.True);
        }

        [Test]
        public void CanRemoveAt_ByIndex_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];

            var itemRemovedReentrantlyIndex = 4;
            var itemRemovedReentrantly = items[itemRemovedReentrantlyIndex];

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            var collection = new TestObservableSet<int>(items);
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
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemRemoved), Is.False);
            Assert.That(collection.Contains(itemRemovedReentrantly), Is.True);
        }

        [Test]
        public void CannotRemoveAt_ByIndex_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 3;

            var itemRemovedReentrantlyIndex = 4;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

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
        public void CanRemoveWhere_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 2, 6, 7, 8, 9, itemAddedReentrantly];
            var expectedItemsRemovedCount = 3;

            var collection = new TestObservableSet<int>(items);
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

            collectionCount -= expectedItemsRemovedCount;
            collectionCount++;

            Assert.That(hasReentered, Is.True);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(3), Is.False);
            Assert.That(collection.Contains(4), Is.False);
            Assert.That(collection.Contains(5), Is.False);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.True);
        }

        [Test]
        public void CanRemoveWhere_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 2, 6, 7, 8, 9];
            var expectedItemsRemovedCount = 3;

            var collection = new TestObservableSet<int>(items);
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
        public void CannotRemoveWhere_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 2, 6, 7, 8, 9];

            var collection = new TestObservableSet<int>(items);
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
        public void CanMove_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableSet()
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

            var collection = new TestObservableSet<int>(items);
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
            // Move
            //
            Assert.DoesNotThrow(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            collectionCount += itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanMove_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemMovedIndexOld = 3;
            var itemMovedIndexNew = 4;

            var itemMovedReentrantlyIndexOld = 5;
            var itemMovedReentrantlyIndexNew = 6;

            var expectedItems = items.ToList();

            var itemMoved = expectedItems[itemMovedIndexOld];

            expectedItems.RemoveAt(itemMovedIndexOld);
            expectedItems.Insert(itemMovedIndexNew, itemMoved);

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

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
                        collection.Move(itemMovedReentrantlyIndexOld, itemMovedReentrantlyIndexNew);
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

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));
        }

        [Test]
        public void CannotMove_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemMovedIndexOld = 3;
            var itemMovedIndexNew = 4;

            var itemMovedReentrantlyIndexOld = 5;
            var itemMovedReentrantlyIndexNew = 6;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

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
        public void CanClear_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            int[] expectedItems = [itemAddedReentrantly];

            var collection = new TestObservableSet<int>(items);
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
            // Clear
            //
            Assert.DoesNotThrow(
                collection.Clear);

            collectionCount = itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));
        }

        [Test]
        public void CanClear_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            //int[] expectedItems = [];

            var collection = new TestObservableSet<int>(items);
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
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotClear_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            //var collectionCount = 0;

            var collection = new TestObservableSet<int>(items);
            //var collectionCount = collection.Count;

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
        public void CanUnionWith_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [10, 11];

            var itemAddedReentrantly = 999;
            var itemsAddedCount = itemsOther.Length;
            var itemsAddedReentrantlyCount = 1;

            int[] expectedItems = [.. items, .. itemsOther, itemAddedReentrantly];

            var collection = new TestObservableSet<int>(items);
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
            // UnionWith
            //
            Assert.DoesNotThrow(
                () => collection.UnionWith(itemsOther));

            collectionCount += itemsAddedCount + itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemAddedReentrantly), Is.True);
        }

        [Test]
        public void CanUnionWith_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [10, 11];

            var itemAddedReentrantly = 999;
            var itemsAddedCount = itemsOther.Length;

            int[] expectedItems = [.. items, .. itemsOther];

            var collection = new TestObservableSet<int>(items);
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
            // UnionWith
            //
            Assert.DoesNotThrow(
                () => collection.UnionWith(itemsOther));

            collectionCount += itemsAddedCount;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotUnionWith_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [10, 11];

            var itemAddedReentrantly = 999;

            int[] expectedItems = [.. items, .. itemsOther];

            var collection = new TestObservableSet<int>(items);
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
            // UnionWith
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.UnionWith(itemsOther));

            Assert.That(exception, Is.Not.Null);

            collectionCount += itemsOther.Length;

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.False);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }



        [Test]
        public void CanSymmetricExceptWith_WithReentrantMutationAndSingleSubscriber_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [2, 4, 10, 11];

            var itemAddedReentrantly = 999;
            var itemsAddedReentrantlyCount = 1;

            int[] expectedItems = [0, 1, 3, 5, 6, 7, 8, 9, 10, 11, itemAddedReentrantly];

            var collection = new TestObservableSet<int>(items);
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
            // SymmetricExceptWith
            //
            Assert.DoesNotThrow(
                () => collection.SymmetricExceptWith(itemsOther));

            collectionCount += itemsAddedReentrantlyCount;

            Assert.That(hasReentered, Is.True);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(2), Is.False);
            Assert.That(collection.Contains(4), Is.False);
            Assert.That(collection.Contains(10), Is.True);
            Assert.That(collection.Contains(11), Is.True);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.True);
        }

        [Test]
        public void CanSymmetricExceptWith_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [2, 4, 10, 11];

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 3, 5, 6, 7, 8, 9, 10, 11];

            var collection = new TestObservableSet<int>(items);
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
            // SymmetricExceptWith
            //
            Assert.DoesNotThrow(
                () => collection.SymmetricExceptWith(itemsOther));

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.True);
            Assert.That(reentrantException, Is.Not.Null);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(2), Is.False);
            Assert.That(collection.Contains(4), Is.False);
            Assert.That(collection.Contains(10), Is.True);
            Assert.That(collection.Contains(11), Is.True);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        [Test]
        public void CannotSymmetricExceptWith_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsOther = [2, 4, 10, 11];

            var itemAddedReentrantly = 999;

            int[] expectedItems = [0, 1, 3, 5, 6, 7, 8, 9, 10, 11];

            var collection = new TestObservableSet<int>(items);
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
            // SymmetricExceptWith
            //
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.SymmetricExceptWith(itemsOther));

            Assert.That(exception, Is.Not.Null);

            Assert.That(hasReentered, Is.True);
            Assert.That(hasObserved, Is.False);

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(collection.Contains(2), Is.False);
            Assert.That(collection.Contains(4), Is.False);
            Assert.That(collection.Contains(10), Is.True);
            Assert.That(collection.Contains(11), Is.True);
            Assert.That(collection.Contains(itemAddedReentrantly), Is.False);
        }

        #endregion
    }
}
