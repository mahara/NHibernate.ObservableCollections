using Iesi.Collections.Generic;

namespace NHibernate.ObservableCollections.PerformanceTests
{
    public class ObservableCollection_Benchmarks
    {
        [Params(100, 1_000, 10_000)]
        //[Params(1_000, 10_000, 100_000)]
        public int ItemsCount;

        private List<int> _items = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _items = [.. Enumerable.Range(1, ItemsCount)];
        }

        [Benchmark]
        public void SystemObservableCollection_Add()
        {
            System.Collections.ObjectModel.ObservableCollection<int> collection = [];

            foreach (var item in _items)
            {
                collection.Add(item);
            }
        }

        [Benchmark]
        public void ObservableCollection_Add()
        {
            ObservableCollection<int> collection = [];

            foreach (var item in _items)
            {
                collection.Add(item);
            }
        }

        [Benchmark]
        public void ObservableCollection_AddRange()
        {
            ObservableCollection<int> collection = [];

            collection.AddRange(_items);
        }
    }

    public class ObservableCollection_ReplaceRange_Benchmarks
    {
        [Params(100, 1_000, 10_000)]
        //[Params(1_000, 10_000, 100_000)]
        public int ItemsCount;

        private List<int> _items = null!;
        private ObservableCollection<int> _collection = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _items = [.. Enumerable.Range(1, ItemsCount)];
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _collection = [.. _items];
        }

        [Benchmark]
        public void ObservableCollection_ReplaceRange()
        {
            int index, count;
            index = count = ItemsCount * (40 / 100);

            _collection.ReplaceRange(index, count, _items);
        }
    }
}
