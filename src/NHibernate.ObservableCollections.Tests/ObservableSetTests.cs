namespace Iesi.Collections.Generic.Tests
{
    [TestFixture(EventNotificationOrderMode.Strict)]
    [TestFixture(EventNotificationOrderMode.Relaxed)]
    public class ObservableSetTests : TestBase
    {
        private readonly List<int> _items = [];

        #region Setup

        public ObservableSetTests(EventNotificationOrderMode eventNotificationOrderMode) :
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void Constructor_RemovesDuplicatesAndPreservesFirstOccurrenceOrder()
        {
            List<int> items = [1, 2, 1, 3, 2, 4];
            //var itemsCount = items.Count;

            int[] expectedItems = [1, 2, 3, 4];

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void Constructor_RemovesDuplicatesUsingCustomComparerAndPreservesFirstOccurrenceOrder()
        {
            IEqualityComparer<string> itemEqualityComparer = StringComparer.OrdinalIgnoreCase;

            List<string> items = ["ABC", "abc", "DEF", "def", "GHI"];

            string[] expectedItems = ["ABC", "DEF", "GHI"];

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

            //
            // Constructor
            //
            var collection = new TestObservableSet<string>(items, itemEqualityComparer);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            var events = collection.CollectionChangedEventArgsList;
            var propertyChangingEvents = collection.PropertyChangingEventArgsList;
            var propertyChangedEvents = collection.PropertyChangedEventArgsList;

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            int[] expectedAdd_Event_NewItems = [itemAdded];
            var expectedAdd_Event_NewItemsCount = expectedAdd_Event_NewItems.Length;
            var expectedAdd_Event_NewStartingIndex = 0;
            var expectedEventCount = itemsAddedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

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
        public void CanAdd_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemAdded = items[^1] + 1;
            var itemsAddedCount = 1;

            int[] expectedItems = [.. items, itemAdded];

            int[] expectedAdd_Event_NewItems = [itemAdded];
            var expectedAdd_Event_NewItemsCount = expectedAdd_Event_NewItems.Length;
            var expectedAdd_Event_NewStartingIndex = itemsCount;
            var expectedEventCount = itemsAddedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(@event.OldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedAdd_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedAdd_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedAdd_Event_NewStartingIndex));
        }

        [Test]
        public void CannotAdd_DuplicateItem_NonEmptyObservableSet()
        {
            int[] items = [_items[0]];
            //var itemsCount = items.Count;

            var itemAdded = items[0];
            var itemsAddedCount = 0;

            int[] expectedItems = [itemAdded];

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            int[] expectedInsert_Event_NewItems = [itemInserted];
            var expectedInsert_Event_NewItemsCount = expectedInsert_Event_NewItems.Length;
            var expectedInsert_Event_NewStartingIndex = itemInsertedIndex;
            var expectedEventCount = itemsInsertedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedInsert_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedInsert_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedInsert_Event_NewStartingIndex));
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

            int[] expectedInsert_Event_NewItems = [itemInserted];
            var expectedInsert_Event_NewItemsCount = expectedInsert_Event_NewItems.Length;
            var expectedInsert_Event_NewStartingIndex = itemInsertedIndex;
            var expectedEventCount = itemsInsertedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedInsert_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedInsert_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedInsert_Event_NewStartingIndex));
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

            int[] expectedInsert_Event_NewItems = [itemInserted];
            var expectedInsert_Event_NewItemsCount = expectedInsert_Event_NewItems.Length;
            var expectedInsert_Event_NewStartingIndex = itemInsertedIndex;
            var expectedEventCount = itemsInsertedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedInsert_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedInsert_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedInsert_Event_NewStartingIndex));
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

            int[] expectedInsert_Event_NewItems = [itemInserted];
            var expectedInsert_Event_NewItemsCount = expectedInsert_Event_NewItems.Length;
            var expectedInsert_Event_NewStartingIndex = itemInsertedIndex;
            var expectedEventCount = itemsInsertedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Null);

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedInsert_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedInsert_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedInsert_Event_NewStartingIndex));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            int[] expectedRemove_Event_OldItems = [itemRemoved];
            var expectedRemove_Event_OldItemsCount = expectedRemove_Event_OldItems.Length;
            var expectedRemove_Event_OldStartingIndex = itemRemovedIndex;
            var expectedEventCount = itemsRemovedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[0];

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
        public void CanRemove_FirstItem_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemovedIndex = 0;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            int[] expectedRemove_Event_OldItems = [itemRemoved];
            var expectedRemove_Event_OldItemsCount = expectedRemove_Event_OldItems.Length;
            var expectedRemove_Event_OldStartingIndex = itemRemovedIndex;
            var expectedEventCount = itemsRemovedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
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
        public void CanRemove_LastItem_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemRemovedIndex = itemsCount - 1;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var expectedItems = items.Where((item, index) => index != itemRemovedIndex)
                                     .ToArray();

            int[] expectedRemove_Event_OldItems = [itemRemoved];
            var expectedRemove_Event_OldItemsCount = expectedRemove_Event_OldItems.Length;
            var expectedRemove_Event_OldStartingIndex = itemRemovedIndex;
            var expectedEventCount = itemsRemovedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
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
        public void CannotRemove_MissingItem_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemRemoved = items[^1] + 1;
            var itemsRemovedCount = 0;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            string[] expectedRemove_Event_OldItems = [item];
            var expectedRemove_Event_OldItemsCount = expectedRemove_Event_OldItems.Length;
            var expectedRemove_Event_OldStartingIndex = itemRemovedIndex;
            var expectedEventCount = itemsRemovedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<string>(), Is.EqualTo(expectedRemove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemove_Event_OldStartingIndex));

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

            int[] expectedRemoveAt_Event_OldItems = [itemRemoved];
            var expectedRemoveAt_Event_OldItemsCount = expectedRemoveAt_Event_OldItems.Length;
            var expectedRemoveAt_Event_OldStartingIndex = itemRemovedIndex;
            var expectedEventCount = itemsRemovedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemoveAt_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveAt_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemoveAt_Event_OldStartingIndex));

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

            int[] expectedRemoveAt_Event_OldItems = [itemRemoved];
            var expectedRemoveAt_Event_OldItemsCount = expectedRemoveAt_Event_OldItems.Length;
            var expectedRemoveAt_Event_OldStartingIndex = itemRemovedIndex;
            var expectedEventCount = itemsRemovedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemoveAt_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveAt_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemoveAt_Event_OldStartingIndex));

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

            int[] expectedRemoveAt_Event_OldItems = [itemRemoved];
            var expectedRemoveAt_Event_OldItemsCount = expectedRemoveAt_Event_OldItems.Length;
            var expectedRemoveAt_Event_OldStartingIndex = itemRemovedIndex;
            var expectedEventCount = itemsRemovedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemoveAt_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedRemoveAt_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemoveAt_Event_OldStartingIndex));

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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotRemoveAt_WithIndexEqualToCount_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemRemovedIndex = itemsCount;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "index";

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotRemoveAt_EmptyObservableSet()
        {
            var itemRemovedIndex = 0;

            var expectedExceptionParameterName = "index";

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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
            // RemoveAt
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.RemoveAt(itemRemovedIndex));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            int[] expectedMove_Event_NewItems = [itemMoved];
            var expectedMove_Event_NewItemsCount = expectedMove_Event_NewItems.Length;
            var expectedMove_Event_NewStartingIndex = itemMovedIndexNew;

            int[] expectedMove_Event_OldItems = [itemMoved];
            var expectedMove_Event_OldItemsCount = expectedMove_Event_OldItems.Length;
            var expectedMove_Event_OldStartingIndex = itemMovedIndexOld;

            var expectedEventCount = 1;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

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
            // Move
            //
            collection.Move(itemMovedIndexOld, itemMovedIndexNew);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Move));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedMove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedMove_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedMove_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedMove_Event_NewStartingIndex));
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

            int[] expectedMove_Event_NewItems = [itemMoved];
            var expectedMove_Event_NewItemsCount = expectedMove_Event_NewItems.Length;
            var expectedMove_Event_NewStartingIndex = itemMovedIndexNew;

            int[] expectedMove_Event_OldItems = [itemMoved];
            var expectedMove_Event_OldItemsCount = expectedMove_Event_OldItems.Length;
            var expectedMove_Event_OldStartingIndex = itemMovedIndexOld;

            var expectedEventCount = 1;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

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
            // Move
            //
            collection.Move(itemMovedIndexOld, itemMovedIndexNew);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Move));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedMove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedMove_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedMove_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedMove_Event_NewStartingIndex));
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

            int[] expectedMove_Event_NewItems = [itemMoved];
            var expectedMove_Event_NewItemsCount = expectedMove_Event_NewItems.Length;
            var expectedMove_Event_NewStartingIndex = itemMovedIndexNew;

            int[] expectedMove_Event_OldItems = [itemMoved];
            var expectedMove_Event_OldItemsCount = expectedMove_Event_OldItems.Length;
            var expectedMove_Event_OldStartingIndex = itemMovedIndexOld;

            var expectedEventCount = 1;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

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
            // Move
            //
            collection.Move(itemMovedIndexOld, itemMovedIndexNew);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Move));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedMove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedMove_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedMove_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedMove_Event_NewStartingIndex));
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

            int[] expectedMove_Event_NewItems = [itemMoved];
            var expectedMove_Event_NewItemsCount = expectedMove_Event_NewItems.Length;
            var expectedMove_Event_NewStartingIndex = itemMovedIndexNew;

            int[] expectedMove_Event_OldItems = [itemMoved];
            var expectedMove_Event_OldItemsCount = expectedMove_Event_OldItems.Length;
            var expectedMove_Event_OldStartingIndex = itemMovedIndexOld;

            var expectedEventCount = 1;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventPropertyNames = IndexerPropertyChangedEventPropertyNames;

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
            // Move
            //
            collection.Move(itemMovedIndexOld, itemMovedIndexNew);

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Move));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedMove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedMove_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems, Has.Count.EqualTo(expectedMove_Event_NewItemsCount));
            Assert.That(eventNewItems!.Cast<int>(), Is.EqualTo(expectedMove_Event_NewItems));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(expectedMove_Event_NewStartingIndex));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotMove_EmptyObservableSet()
        {
            var itemMovedIndexOld = 0;
            var itemMovedIndexNew = 0;

            var expectedExceptionParameterName = "oldIndex";

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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
            // Move
            //
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => collection.Move(itemMovedIndexOld, itemMovedIndexNew));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        #endregion



        #region Clear

        [Test]
        public void CanClear_EmptyObservableSet()
        {
            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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
            // Clear
            //
            collection.Clear();

            Assert.That(collection, Is.Empty);
            Assert.That(collection.ToArray(), Is.Empty);

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CanClear_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] itemsRemoved = [.. items];
            var itemsRemovedCount = itemsRemoved.Length;

            //int[] expectedItems = [];

            var expectedEventCount = 1;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

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

            int[] expectedRemove_Event_OldItems = [3, 4, 5];
            var expectedRemove_Event_OldItemsCount = expectedRemove_Event_OldItems.Length;
            var expectedRemove_Event_OldStartingIndex = 3;
            var expectedEventCount = 1;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
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
        public void CanRemoveWhere_WithMultipleClusters_NonEmptyObservableSet()
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
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
        public void CanRemoveWhere_AllItems_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            //int[] expectedItems = [];
            var expectedItemsRemovedCount = itemsCount;

            var expectedEventCount = 1;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotRemoveWhere_EmptyObservableSet()
        {
            //int[] expectedItems = [];
            var expectedItemsRemovedCount = 0;

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        [Test]
        public void CannotRemoveWhere_WithNullMatch_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            Predicate<int>? match = null;

            int[] expectedItems = [.. items];
            var expectedExceptionParameterName = "match";

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => collection.RemoveWhere(match!));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(expectedExceptionParameterName));

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
            Assert.That(collection.ToArray(), Is.EqualTo(expectedItems));

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            string[] expectedRemove_Event_OldItems = [itemStored];
            var expectedRemove_Event_OldItemsCount = expectedRemove_Event_OldItems.Length;
            var expectedRemove_Event_OldStartingIndex = 0;
            var expectedEventCount = expectedItemsRemovedCount;
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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            AssertPropertyChangingEvents(propertyChangingEvents, expectedPropertyChangingEventPropertyNames);
            AssertPropertyChangedEvents(propertyChangedEvents, expectedPropertyChangedEventPropertyNames);

            var @event = events[expectedEventCount - 1];

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems, Has.Count.EqualTo(expectedRemove_Event_OldItemsCount));
            Assert.That(eventOldItems!.Cast<string>(), Is.EqualTo(expectedRemove_Event_OldItems));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(expectedRemove_Event_OldStartingIndex));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Null);
        }

        [Test]
        public void CannotRemoveWhere_WhenMatchThrows_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            int[] expectedItems = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
        }

        #endregion



        #region CopyTo

        [Test]
        public void CanCopyTo_PreservesInsertionOrder_NonEmptyObservableSet()
        {
            int[] items = [7, 3, 9, 1, 5, 2];
            //var itemsCount = items.Length;

            int[] expectedItems = [.. items];
            int[] expectedArrayDestination = [.. items];

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            var expectedEventCount = 0;
            var expectedPropertyChangingEventCount = 0;
            var expectedPropertyChangedEventCount = 0;

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

            Assert.That(events, Has.Count.EqualTo(expectedEventCount));
            Assert.That(propertyChangingEvents, Has.Count.EqualTo(expectedPropertyChangingEventCount));
            Assert.That(propertyChangedEvents, Has.Count.EqualTo(expectedPropertyChangedEventCount));
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

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
            Assert.That(collection, Is.EqualTo(expectedItems));
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
        public void CanClear_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            //int[] expectedItems = [];

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
        public void CannotClear_WithReentrantMutationAndMultipleSubscribers_NonEmptyObservableSet()
        {
            var items = _items;
            //var itemsCount = items.Count;

            var itemAddedReentrantly = 999;

            var collection = new TestObservableSet<int>(items);

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

        #endregion
    }
}
