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
            Assert.That(args.NewStartingIndex, Is.EqualTo(itemsCount));

            itemsCount += addedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }

        [Test]
        public void CanRemove_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var removedItemIndex = 3;

            var collection = new ObservableCollection<int>(_items);
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
            Assert.That(args.OldStartingIndex, Is.EqualTo(removedItemIndex));

            itemsCount -= removedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }

        [Test]
        public void CanAddRange_EmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var collection = new ObservableCollection<int>();
            var itemsCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.AddRange(_items);
            var addedItemsCount = _items.Count;

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var addedItems = args.NewItems! as IEnumerable;

            Assert.That(addedItems, Is.Not.Null);
            Assert.That(addedItems.Cast<object>().Count(), Is.EqualTo(addedItemsCount));
            Assert.That(args.NewStartingIndex, Is.EqualTo(itemsCount));

            itemsCount += addedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }

        [Test]
        public void CanAddRange_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var collection = new ObservableCollection<int>(_items);
            var itemsCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.AddRange(_items);
            var addedItemsCount = _items.Count;

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var addedItems = args.NewItems! as IEnumerable;

            Assert.That(addedItems, Is.Not.Null);
            Assert.That(addedItems.Cast<object>().Count(), Is.EqualTo(addedItemsCount));
            Assert.That(args.NewStartingIndex, Is.EqualTo(itemsCount));

            itemsCount += addedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }

        [Test]
        public void CanRemoveRange_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var removedItemsIndex = 3;
            var removedItemsCount = 4;

            var collection = new ObservableCollection<int>(_items);
            var itemsCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.RemoveRange(_items.GetRange(removedItemsIndex, removedItemsCount));

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var removedItems = args.OldItems! as IEnumerable;

            Assert.That(removedItems, Is.Not.Null);
            Assert.That(removedItems.Cast<object>().Count(), Is.EqualTo(removedItemsCount));
            Assert.That(args.OldStartingIndex, Is.GreaterThanOrEqualTo(0));

            itemsCount -= removedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }

        [Test]
        public void CanRemoveRangeAll_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var removedItemsIndex = 0;
            var removedItemsCount = _items.Count;

            var collection = new ObservableCollection<int>(_items);
            var itemsCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.RemoveRange(_items.GetRange(removedItemsIndex, removedItemsCount));

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));

            itemsCount -= removedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }

        [Test]
        public void CanRemoveRangeByIndexAndCount_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var removedItemsIndex = 3;
            var removedItemsCount = 4;

            var collection = new ObservableCollection<int>(_items);
            var itemsCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.RemoveRange(removedItemsIndex, removedItemsCount);

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));

            var removedItems = args.OldItems! as IEnumerable;

            Assert.That(removedItems, Is.Not.Null);
            Assert.That(removedItems.Cast<object>().Count(), Is.EqualTo(removedItemsCount));
            Assert.That(args.OldStartingIndex, Is.EqualTo(removedItemsIndex));

            itemsCount -= removedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }

        [Test]
        public void CanRemoveRangeAllByIndexAndCount_NonEmptyObservableCollection()
        {
            NotifyCollectionChangedEventArgs args = null!;
            var notificationCount = 0;

            var removedItemsIndex = 0;
            var removedItemsCount = _items.Count;

            var collection = new ObservableCollection<int>(_items);
            var itemsCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            using var _ = new CollectionChangedEventSubscription(
                collection,
                (o, e) =>
                {
                    args = e;
                    notificationCount++;
                });

            collection.RemoveRange(removedItemsIndex, removedItemsCount);

            Assert.That(args, Is.Not.Null);
            Assert.That(notificationCount, Is.EqualTo(1));
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));

            itemsCount -= removedItemsCount;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));
        }
    }
}
