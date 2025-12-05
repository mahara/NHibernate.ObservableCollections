internal class Program
{
    private static void Main(string[] args)
    {
        var baseJob = Job.Default;

        var jobNet100 = baseJob.WithRuntime(CoreRuntime.Core10_0).WithBaseline(true);
        var jobNet90 = baseJob.WithRuntime(CoreRuntime.Core90);
        var jobNet80 = baseJob.WithRuntime(CoreRuntime.Core80);
        var jobNet48 = baseJob.WithRuntime(ClrRuntime.Net48);

        var baseConfig = DefaultConfig.Instance;

        var config = baseConfig
            .AddJob(jobNet100)
            .AddJob(jobNet90)
            .AddJob(jobNet80)
            .AddJob(jobNet48)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddExporter(RPlotExporter.Default);



        //BenchmarkRunner.Run<LoopsBenchmarks>(config, args);

        //BenchmarkRunner.Run<ObservableCollection_Benchmarks>(config, args);
        //BenchmarkRunner.Run<ObservableCollection_ReplaceRange_Benchmarks>(config, args);
    }
}
