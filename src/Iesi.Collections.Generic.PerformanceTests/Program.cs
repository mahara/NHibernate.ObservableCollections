internal class Program
{
    private static void Main(string[] args)
    {
        var baseJob = Job.Default;

        var baseConfig = DefaultConfig.Instance
                                      .AddExporter(RPlotExporter.Default);

        //var baseConfig = ManualConfig.CreateEmpty()
        //                             .AddLogger(BenchmarkDotNet.Loggers.ConsoleLogger.Default)
        //                             .AddExporter(BenchmarkDotNet.Exporters.MarkdownExporter.Default)
        //                             .AddColumnProvider(BenchmarkDotNet.Columns.DefaultColumnProviders.Instance);



        var type = typeof(Program);

        BenchmarkExecutor.Execute(args, type, baseJob, baseConfig);
    }
}
