namespace Iesi.Collections.Generic.Tests
{
    [TestFixture]
    public class ObservableSetTests
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
            var notificationCount = itemsAddedCount;

            var collection = new TestObservableSet<int>();
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(0));

            var argsList = collection.CollectionChangedEventArgsList;

            Assert.That(argsList, Has.Count.EqualTo(0));

            collection.Add(itemAdded);

            Assert.That(argsList, Has.Count.EqualTo(notificationCount));

            var args = argsList[0];

            Assert.That(args, Is.Not.Null);
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            var argsNewItems = args.NewItems;

            Assert.That(argsNewItems, Is.Not.Null);
            Assert.That(argsNewItems, Has.Count.EqualTo(itemsAddedCount));
            Assert.That(args.NewStartingIndex, Is.EqualTo(collectionCount));

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
            var notificationCount = itemsRemovedCount;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var argsList = collection.CollectionChangedEventArgsList;

            Assert.That(argsList, Has.Count.EqualTo(0));

            collection.Remove(itemRemoved);

            Assert.That(argsList, Has.Count.EqualTo(notificationCount));

            var args = argsList[0];

            Assert.That(args, Is.Not.Null);
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));

            var argsOldItems = args.OldItems;

            Assert.That(argsOldItems, Is.Not.Null);
            Assert.That(argsOldItems, Has.Count.EqualTo(itemsRemovedCount));
            Assert.That(args.OldStartingIndex, Is.EqualTo(itemRemovedIndex));

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }

        [Test]
        public void CanClear_NonEmptySet()
        {
            var items = _items;
            var itemsCount = items.Count;

            var itemsRemovedCount = itemsCount;
            var notificationCount = 1;

            var collection = new TestObservableSet<int>(items);
            var collectionCount = collection.Count;

            Assert.That(collection, Has.Count.EqualTo(itemsCount));

            var argsList = collection.CollectionChangedEventArgsList;

            Assert.That(argsList, Has.Count.EqualTo(0));

            collection.Clear();

            Assert.That(argsList, Has.Count.EqualTo(notificationCount));

            var args = argsList[0];

            Assert.That(args, Is.Not.Null);
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(args.OldItems, Is.Null);
            Assert.That(args.NewItems, Is.Null);

            collectionCount -= itemsRemovedCount;

            Assert.That(collection, Has.Count.EqualTo(collectionCount));
        }
    }
}
