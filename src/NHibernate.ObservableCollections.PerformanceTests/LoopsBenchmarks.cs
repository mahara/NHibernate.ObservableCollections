using BenchmarkDotNet.Attributes;

namespace NHibernate.ObservableCollections.PerformanceTests;

/// <summary>
/// </summary>
/// <remarks>
///     REFERENCES:
///     -   <see href="https://stackoverflow.com/questions/365615/in-net-which-loop-runs-faster-for-or-foreach" />
/// </remarks>
public class LoopsBenchmarks
{
#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
    [Params(10_000, 10_000_000)]
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
    public int ItemsCount;

    private List<int> _items = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _items = [.. Enumerable.Range(1, ItemsCount)];
    }

    [Benchmark]
    public void ListFor()
    {
        for (var i = 0; i < _items.Count; i++)
        {
            _ = _items[i];
        }
    }

    [Benchmark]
    public void ListForEach()
    {
        foreach (var item in _items)
        {
            _ = item;
        }
    }
}
