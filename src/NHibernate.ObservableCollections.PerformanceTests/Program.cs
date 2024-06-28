using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

using NHibernate.ObservableCollections.PerformanceTests;

internal class Program
{
    public static void Main(string[] args)
    {
        var job = Job.Default;
        var jobNet80 = job.WithRuntime(CoreRuntime.Core80).WithBaseline(true);
        var jobNet70 = job.WithRuntime(CoreRuntime.Core70);
        var jobNet60 = job.WithRuntime(CoreRuntime.Core60);
        var jobNet48 = job.WithRuntime(ClrRuntime.Net48)
                          .WithToolchain(InProcessEmitToolchain.Instance);

        var config = DefaultConfig.Instance
            .AddJob(jobNet80)
            .AddJob(jobNet70)
            .AddJob(jobNet48)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddExporter(RPlotExporter.Default);

        //BenchmarkRunner.Run<LoopsBenchmarks>(config, args);
        BenchmarkRunner.Run<ObservableCollection_Benchmarks>(config, args);
        BenchmarkRunner.Run<ObservableCollection_ReplaceRange_Benchmarks>(config, args);
    }
}
