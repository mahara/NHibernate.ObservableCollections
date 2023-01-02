namespace Iesi.Collections.Generic.Tests
{
    [TestFixture]
    public class ObservableSetTest
    {
        private readonly List<int> _items = new();

        [OneTimeSetUp]
        public void SetupFixture()
        {
            _items.Clear();
            _items.AddRange(Enumerable.Range(0, 10));
        }

        [Test]
        public void CanAdd_EmptyObservableSet()
        {
            NotifyCollectionChangedEventArgs @event = null!;
            var eventsCount = 0;

            var collection = new ObservableSet<int>();
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
        public void CanRemove_NonEmptyObservableSet()
        {
            NotifyCollectionChangedEventArgs @event = null!;
            var eventsCount = 0;

            var itemRemovedIndex = 3;

            var collection = new ObservableSet<int>(_items);
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

            var itemsRemoved = @event.OldItems;

            Assert.That(itemsRemoved, Is.Not.Null);
            Assert.That(itemsRemoved.Count, Is.EqualTo(itemsRemovedCount));
            Assert.That(@event.OldStartingIndex, Is.EqualTo(itemRemovedIndex));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }
    }
}
