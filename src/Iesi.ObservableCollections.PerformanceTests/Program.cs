internal static class Program
{
    private static void Main(string[] args)
    {
        var baseConfig = DefaultConfig.Instance;

        //var baseConfig = ManualConfig.CreateEmpty()
        //                             .AddLogger(BenchmarkDotNet.Loggers.ConsoleLogger.Default)
        //                             .AddExporter(BenchmarkDotNet.Exporters.MarkdownExporter.Default)
        //                             .AddColumnProvider(BenchmarkDotNet.Columns.DefaultColumnProviders.Instance);



        var baseJob = Job.Default;



        var baseFeatures = BenchmarkFeatures.None;



        var type = typeof(Program);

        BenchmarkExecutor.Execute(args, type, baseConfig, baseJob, baseFeatures);
    }
}
