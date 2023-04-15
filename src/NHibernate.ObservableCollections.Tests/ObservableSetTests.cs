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
            var itemsCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.Add(_items[0]);
            var addedItemsCount = 1;

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var addedItems = args.NewItems! as IEnumerable;

            Assert.That(addedItems, Is.Not.Null);
            Assert.That(addedItems.Cast<object>().Count(), Is.EqualTo(addedItemsCount));
            Assert.That(args.NewStartingIndex, Is.EqualTo(-1));

            itemsCount += addedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }

        [Test]
        public void CanRemove_NonEmptyObservableSet()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var removedItemIndex = 3;

            var collection = new ObservableSet<int>(_items);
            var itemsCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.Remove(_items[removedItemIndex]);
            var removedItemsCount = 1;

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var removedItems = args.OldItems! as IEnumerable;

            Assert.That(removedItems, Is.Not.Null);
            Assert.That(removedItems.Cast<object>().Count(), Is.EqualTo(removedItemsCount));
            Assert.That(args.OldStartingIndex, Is.EqualTo(-1));

            itemsCount -= removedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }
    }
}
