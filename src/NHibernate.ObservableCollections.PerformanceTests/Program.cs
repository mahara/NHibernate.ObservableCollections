using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Running;

using NHibernate.ObservableCollections.PerformanceTests;

internal class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddExporter(RPlotExporter.Default);

        //BenchmarkRunner.Run<LoopsBenchmarks>(config, args);
        BenchmarkRunner.Run<ObservableCollection_Benchmarks>(config, args);
        BenchmarkRunner.Run<ObservableCollection_ReplaceRange_Benchmarks>(config, args);
    }
}
