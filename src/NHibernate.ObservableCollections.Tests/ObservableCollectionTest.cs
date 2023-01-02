namespace Iesi.Collections.Tests.Generic
{
    [TestFixture]
    public class ObservableCollectionTest
    {
        private readonly List<int> _items = new();

        [OneTimeSetUp]
        public void SetupFixture()
        {
            _items.Clear();
            _items.AddRange(Enumerable.Range(0, 10));
        }

        [Test]
        public void CanAdd_EmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs @event = null!;
            var eventsCount = 0;

            var collection = new ObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    @event = e;
                    eventsCount++;
                });

            collection.Add(_items[0]);
            var itemsAddedCount = 1;

            Assert.That(@event, Is.Not.Null);
            Assert.That(eventsCount, Is.EqualTo(1));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems.Count, Is.EqualTo(itemsAddedCount));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(collectionCount));

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanRemove_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs @event = null!;
            var eventsCount = 0;

            var itemRemovedIndex = 3;

            var collection = new ObservableCollection<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    @event = e;
                    eventsCount++;
                });

            collection.Remove(_items[itemRemovedIndex]);
            var itemsRemovedCount = 1;

            Assert.That(@event, Is.Not.Null);
            Assert.That(eventsCount, Is.EqualTo(1));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems.Count, Is.EqualTo(itemsRemovedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(itemRemovedIndex));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanAddRange_EmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs @event = null!;
            var eventsCount = 0;

            var collection = new ObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    @event = e;
                    eventsCount++;
                });

            collection.AddRange(_items);
            var itemsAddedCount = _items.Count;

            Assert.That(@event, Is.Not.Null);
            Assert.That(eventsCount, Is.EqualTo(1));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems.Count, Is.EqualTo(itemsAddedCount));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(collectionCount));

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanAddRange_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs @event = null!;
            var eventsCount = 0;

            var collection = new ObservableCollection<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    @event = e;
                    eventsCount++;
                });

            collection.AddRange(_items);
            var itemsAddedCount = _items.Count;

            Assert.That(@event, Is.Not.Null);
            Assert.That(eventsCount, Is.EqualTo(1));

            var eventNewItems = @event.NewItems;

            Assert.That(eventNewItems, Is.Not.Null);
            Assert.That(eventNewItems.Count, Is.EqualTo(itemsAddedCount));
            Assert.That(@event.NewStartingIndex, Is.EqualTo(collectionCount));

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanRemoveRange_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs @event = null!;
            var eventsCount = 0;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;

            var collection = new ObservableCollection<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    @event = e;
                    eventsCount++;
                });

            collection.RemoveRange(_items.GetRange(itemsRemovedIndex, itemsRemovedCount));

            Assert.That(@event, Is.Not.Null);
            Assert.That(eventsCount, Is.EqualTo(1));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems.Count, Is.EqualTo(itemsRemovedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(0));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanRemoveRangeByIndexAndCount_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs @event = null!;
            var eventsCount = 0;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;

            var collection = new ObservableCollection<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    @event = e;
                    eventsCount++;
                });

            collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount);

            Assert.That(@event, Is.Not.Null);
            Assert.That(eventsCount, Is.EqualTo(1));

            var eventOldItems = @event.OldItems;

            Assert.That(eventOldItems, Is.Not.Null);
            Assert.That(eventOldItems.Count, Is.EqualTo(itemsRemovedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(itemsRemovedIndex));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }
    }
}
