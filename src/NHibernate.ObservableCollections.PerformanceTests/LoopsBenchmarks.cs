namespace NHibernate.ObservableCollections.PerformanceTests;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

/// <summary>
///
/// </summary>
/// <remarks>
///     REFERENCES:
///     -   <see href="https://stackoverflow.com/questions/365615/in-net-which-loop-runs-faster-for-or-foreach" />
/// </remarks>
[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net48)]
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
        _items = Enumerable.Range(1, ItemsCount).ToList();
    }

    [Benchmark]
    public void ListFor()
    {
        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
        }
    }

    [Benchmark]
    public void ListForEach()
    {
        foreach (var item in _items)
        {
        }
    }
}
