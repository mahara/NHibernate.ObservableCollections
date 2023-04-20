namespace Iesi.Collections.Generic.Tests
{
    [TestFixture]
    public class ObservableCollectionTests
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
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var collection = new ObservableCollection<int>();
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
        public void CanRemove_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var itemRemovedIndex = 3;

            var collection = new ObservableCollection<int>(_items);
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

            var argsOldItems = args.OldItems;

            Assert.That(argsOldItems, Is.Not.Null);
            Assert.That(argsOldItems.Count, Is.EqualTo(itemsRemovedCount));
            Assert.That(args.OldStartingIndex, Is.EqualTo(itemRemovedIndex));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanAddRange_EmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var collection = new ObservableCollection<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.AddRange(_items);
            var itemsAddedCount = _items.Count;

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
        public void CanAddRange_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var collection = new ObservableCollection<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.AddRange(_items);
            var itemsAddedCount = _items.Count;

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
        public void CanRemoveRange_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;

            var collection = new ObservableCollection<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.RemoveRange(_items.GetRange(itemsRemovedIndex, itemsRemovedCount));

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var argsOldItems = args.OldItems;

            Assert.That(argsOldItems, Is.Not.Null);
            Assert.That(argsOldItems.Count, Is.EqualTo(itemsRemovedCount));
            Assert.That(args.OldStartingIndex, Is.GreaterThanOrEqualTo(0));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanRemoveRangeAll_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var itemsRemovedIndex = 0;
            var itemsRemovedCount = _items.Count;

            var collection = new ObservableCollection<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.RemoveRange(_items.GetRange(itemsRemovedIndex, itemsRemovedCount));

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanRemoveRangeByIndexAndCount_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var itemsRemovedIndex = 3;
            var itemsRemovedCount = 4;

            var collection = new ObservableCollection<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount);

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var argsOldItems = args.OldItems;

            Assert.That(argsOldItems, Is.Not.Null);
            Assert.That(argsOldItems.Count, Is.EqualTo(itemsRemovedCount));
            Assert.That(args.OldStartingIndex, Is.EqualTo(itemsRemovedIndex));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanRemoveRangeAllByIndexAndCount_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var itemsRemovedIndex = 0;
            var itemsRemovedCount = _items.Count;

            var collection = new ObservableCollection<int>(_items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.RemoveRange(itemsRemovedIndex, itemsRemovedCount);

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }
    }
}
