namespace Iesi.Collections.Generic.Tests
{
    [TestFixture]
    public class ObservableSetTests
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
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var collection = new ObservableSet<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.Add(_items[0]);
            var itemsAddedCount = 1;

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var argsNewItems = args.NewItems;

            Assert.That(argsNewItems, Is.Not.Null);
            Assert.That(argsNewItems.Count, Is.EqualTo(itemsAddedCount));
            Assert.That(args.NewStartingIndex, Is.EqualTo(collectionCount));

            collectionCount += itemsAddedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanRemove_NonEmptyObservableSet()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var itemRemovedIndex = 3;

            var collection = new ObservableSet<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.Remove(_items[itemRemovedIndex]);
            var itemsRemovedCount = 1;

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var itemsRemoved = args.OldItems;

            Assert.That(itemsRemoved, Is.Not.Null);
            Assert.That(itemsRemoved.Count, Is.EqualTo(itemsRemovedCount));
            Assert.That(args.OldStartingIndex, Is.EqualTo(itemRemovedIndex));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }
    }
}
