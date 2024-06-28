using BenchmarkDotNet.Attributes;

using Iesi.Collections.Generic;

namespace NHibernate.ObservableCollections.PerformanceTests
{
    public class ObservableCollection_Benchmarks
    {
#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
        [Params(1_000, 10_000, 100_000)]
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
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
#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
        [Params(1_000, 10_000, 100_000)]
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
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
            int startIndex, count;
            startIndex = count = ItemsCount * (40 / 100);
            _collection.ReplaceRange(startIndex, count, _items);
        }
    }
}
