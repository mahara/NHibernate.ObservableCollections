using BenchmarkDotNet.Environments;

internal class Program
{
    private static void Main(string[] args)
    {
        var baseJob = Job.Default;

        var jobNet80 = baseJob.WithRuntime(CoreRuntime.Core80).WithBaseline(true);
        var jobNet70 = baseJob.WithRuntime(CoreRuntime.Core70);
        var jobNet60 = baseJob.WithRuntime(CoreRuntime.Core60);
        var jobNet48 = baseJob.WithRuntime(ClrRuntime.Net48);

        var baseConfig = DefaultConfig.Instance;

        var config = baseConfig
            .AddJob(jobNet80)
            .AddJob(jobNet70)
            .AddJob(jobNet60)
            .AddJob(jobNet48)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddExporter(RPlotExporter.Default);



        //BenchmarkRunner.Run<LoopsBenchmarks>(config, args);

        //BenchmarkRunner.Run<ObservableCollection_Benchmarks>(config, args);
        //BenchmarkRunner.Run<ObservableCollection_ReplaceRange_Benchmarks>(config, args);
    }
}
