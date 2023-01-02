namespace Iesi.Collections.Generic.Tests
{
    [TestFixture]
    public class ObservableListTests
    {
        private readonly List<int> _items = new();

        [Test]
        public void CanAddRange()
        {
            var observableList = new ObservableList<int>();

            Assert.That(observableList, Is.Empty);

            observableList.AddRange(_items);

            Assert.That(observableList.Count, Is.EqualTo(_items.Count));
        }

        [Test]
        public void CanRemoveRange()
        {
            var removedItemsIndex = 3;
            var removedItemsCount = 4;

            var observableList = new ObservableList<int>();

            observableList.AddRange(_items);

            Assert.That(observableList.Count, Is.EqualTo(_items.Count));

            observableList.RemoveRange(_items.GetRange(removedItemsIndex, removedItemsCount));

            Assert.That(observableList.Count, Is.EqualTo(_items.Count - removedItemsCount));
        }

        [Test]
        public void CanRemoveRangeByIndexAndCount()
        {
            var removedItemsIndex = 3;
            var removedItemsCount = 4;

            var observableList = new ObservableList<int>();

            observableList.AddRange(_items);

            Assert.That(observableList.Count, Is.EqualTo(_items.Count));

            observableList.RemoveRange(removedItemsIndex, removedItemsCount);

            Assert.That(observableList.Count, Is.EqualTo(_items.Count - removedItemsCount));
        }

        [OneTimeSetUp]
        public void SetupFixture()
        {
            _items.Clear();
            _items.AddRange(Enumerable.Range(0, 10));
        }
    }
}
