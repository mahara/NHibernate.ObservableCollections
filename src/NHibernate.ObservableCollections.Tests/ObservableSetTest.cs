namespace Iesi.Collections.Generic.Tests
{
    [TestFixture]
    public class ObservableSetTest
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
            var items = _items;
            //var itemsCount = items.Count;

            var itemAdded = items[0];
            var itemsAddedCount = 1;

            var eventsCount = itemsAddedCount;

            var collection = new TestObservableSet<int>();
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
        public void CanRemove_NonEmptyObservableSet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemRemovedIndex = 3;
            var itemRemoved = items[itemRemovedIndex];
            var itemsRemovedCount = 1;

            var eventsCount = itemsRemovedCount;

            var collection = new TestObservableSet<int>(items);
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
        public void CanClear_NonEmptySet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsRemovedCount = itemsCount;

            var eventsCount = 1;

            var collection = new TestObservableSet<int>(items);
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
    }
}
