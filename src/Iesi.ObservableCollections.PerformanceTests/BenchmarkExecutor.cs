using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;

namespace Iesi.Collections.PerformanceTests;

[Flags]
public enum BenchmarkFeatures
{
    None = 0,
    Memory = 1 << 0,
    Disassembly = 1 << 1,
    LongRun = 1 << 2,
    Stable = 1 << 3,
    Plotting = 1 << 4,
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class BenchmarkFeaturesAttribute : Attribute
{
    public BenchmarkFeatures Enable { get; }

    public BenchmarkFeatures Disable { get; }

    public BenchmarkFeaturesAttribute(
        BenchmarkFeatures enable = BenchmarkFeatures.None,
        BenchmarkFeatures disable = BenchmarkFeatures.None)
    {
        Enable = enable;
        Disable = disable;
    }
}



public class BenchmarkExecutor
{
    private sealed record RuntimeJobDefinition(
        string RuntimeTfm,
        Func<Job> JobFactory);

    private sealed record ExporterDefinition(
        string Name,
        IExporter Exporter);

    private sealed record BenchmarkCaseSelection(
        string[] FilterPatterns,
        string[] Categories)
    {
        public bool HasFilterPatterns => FilterPatterns.Length > 0;

        public bool HasCategories => Categories.Length > 0;
    }

    private sealed record BenchmarkTypePlan(
        Type BenchmarkType,
        List<Job> EffectiveJobs,
        BenchmarkExecutionGroupKey BenchmarkExecutionGroupKey,
        List<string> BenchmarkCaseDisplayInfos)
    {
        public int BenchmarkCaseCount => BenchmarkCaseDisplayInfos.Count;
    }

    private sealed record BenchmarkExecutionGroup(
        BenchmarkExecutionGroupKey BenchmarkExecutionGroupKey,
        List<BenchmarkTypePlan> BenchmarkTypePlans)
    {
        public int BenchmarkTypeCount => BenchmarkTypePlans.Count;

        public int BenchmarkCaseCount => BenchmarkTypePlans.Sum(p => p.BenchmarkCaseCount);
    }

    private sealed record BenchmarkExecutionGroupKey(
        BenchmarkFeatures EffectiveFeatures,
        bool HasMemoryDiagnoserAttribute,
        bool HasDisassemblyDiagnoserAttribute);



    public static readonly List<Runtime> BaseRuntimes =
    [
        CoreRuntime.Core10_0,
        CoreRuntime.Core90,
        CoreRuntime.Core80,
        ClrRuntime.Net48,
    ];

    public const int Job_LongRun_WarmupCount = 20;

    public const int Job_LongRun_IterationCount = 120;

    public const int DisassemblyDiagnoser_MaxDepth = 4;



    private static readonly StringComparer Type_FullName_StringComparer =
        StringComparer.Ordinal;



    public static void Execute(
        string[] args,
        Type type,
        IConfig baseConfig,
        Job baseJob,
        BenchmarkFeatures baseFeatures)
    {
        Type Type = type;
        List<Type> AllTypes = [.. Type.Assembly.GetTypes().OrderBy(t => t.FullName, Type_FullName_StringComparer)];
        List<Type> AllBenchmarkTypes = [.. AllTypes.Where(IsBenchmarkType).OrderBy(t => t.FullName, Type_FullName_StringComparer)];
        List<RuntimeJobDefinition> BaseRuntimeJobDefinitions = CreateRuntimeJobDefinitions(BaseRuntimes, baseJob);
        Dictionary<string, RuntimeJobDefinition> BaseRuntimeJobDefinitionsByRuntimeTfm = BaseRuntimeJobDefinitions.ToDictionary(rj => rj.RuntimeTfm, StringComparer.Ordinal);



        var runtimeJobDefinitions = ResolveRuntimeJobDefinitions(BaseRuntimeJobDefinitionsByRuntimeTfm, args);



        var features = ResolveFeatures(baseFeatures, args);



        var benchmarkCaseSelection = ResolveBenchmarkCaseSelection(args);



        var benchmarkTypes = AllBenchmarkTypes;

        if (benchmarkTypes.Count == 0)
        {
            Console.WriteLine("No benchmark types found.");

            return;
        }



        var benchmarkTypePlans = BuildBenchmarkTypePlans(
            BaseRuntimeJobDefinitions,
            runtimeJobDefinitions,
            baseConfig,
            features,
            benchmarkTypes,
            benchmarkCaseSelection);

        if (benchmarkTypePlans.Count == 0)
        {
            Console.WriteLine("No benchmark cases matched current filter and/or category.");

            return;
        }



        var benchmarkExecutionGroups = BuildBenchmarkExecutionGroups(benchmarkTypePlans);



        // ---- ANALYSIS ----

        ShowBenchmarkExecutionPlan(
            args,
            BaseRuntimeJobDefinitions,
            runtimeJobDefinitions,
            features,
            benchmarkTypes,
            benchmarkCaseSelection,
            benchmarkExecutionGroups);


        if (IsPreviewMode(args))
        {
            return;
        }



        // ---- EXECUTION ----

        ExecuteBenchmarks(
            baseConfig,
            benchmarkCaseSelection,
            benchmarkExecutionGroups);
    }



    private static List<RuntimeJobDefinition> CreateRuntimeJobDefinitions(
        List<Runtime> baseRuntimes,
        Job baseJob)
    {
        if (baseRuntimes.Count == 0)
        {
            throw new InvalidOperationException("No runtimes defined.");
        }

        var RuntimeJobDefinitions =
            baseRuntimes.Select(r =>
                                new RuntimeJobDefinition(
                                    r.MsBuildMoniker,
                                    () => baseJob.WithRuntime(r)))

                        .ToList();

        return RuntimeJobDefinitions;
    }

    private static List<RuntimeJobDefinition> ResolveRuntimeJobDefinitions(
        Dictionary<string, RuntimeJobDefinition> baseRuntimeJobDefinitionsByRuntimeTfm,
        string[] args)
    {
        var values = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg != "--runtimes" &&
                !arg.StartsWith("--runtimes=", StringComparison.Ordinal))
            {
                continue;
            }

            // --runtimes=net10.0
            // --runtimes="net10.0;net48"
            if (arg.Contains('=', StringComparison.Ordinal))
            {
                var value = arg.Split('=', 2)[1];

                values.AddRange(value.SplitBySemicolon());

                break;
            }

            // --runtimes net10.0
            // --runtimes net10.0 net48
            // --runtimes "net10.0;net48"
            for (int j = i + 1; j < args.Length; j++)
            {
                var next = args[j];

                if (next.StartsWith("--"))
                {
                    break;
                }

                values.AddRange(next.SplitBySemicolon());
            }

            break;
        }

        values = [.. values.Distinct(StringComparer.Ordinal)];

        if (values.Count == 0)
        {
            return [];
        }

        var runtimeJobDefinitions = new List<RuntimeJobDefinition>(values.Count);

        foreach (var runtimeTfm in values)
        {
            if (!baseRuntimeJobDefinitionsByRuntimeTfm.TryGetValue(runtimeTfm, out var runtimeJob))
            {
                throw new ArgumentException($"Unknown runtime TFM: '{runtimeTfm}'. Runtime must exactly match TFM (Target Framework Moniker).");
            }

            runtimeJobDefinitions.Add(runtimeJob);
        }

        return runtimeJobDefinitions;
    }

    private static BenchmarkFeatures ResolveFeatures(
        BenchmarkFeatures baseFeatures,
        string[] args)
    {
        var features = baseFeatures |
                       ParseCliProfileFeatures(args) |
                       ParseCliFeatures(args);

        features = ApplyFeatureOverrides(features, args);

        return features;
    }

    private static BenchmarkFeatures ParseCliProfileFeatures(string[] args)
    {
        string? value = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("--profile=", StringComparison.Ordinal))
            {
                value = arg.Split('=', 2)[1];

                break;
            }

            if (arg == "--profile" && i + 1 < args.Length)
            {
                value = args[i + 1];

                break;
            }
        }

        if (value is null)
        {
            return BenchmarkFeatures.None;
        }

        return value switch
        {
            "jit" => BenchmarkFeatures.Disassembly |
                     BenchmarkFeatures.Stable |
                     BenchmarkFeatures.LongRun,

            "allocation" => BenchmarkFeatures.Memory,

            _ => throw new ArgumentException($"Unknown benchmark profile: '{value}'.")
        };
    }

    private static BenchmarkFeatures ParseCliFeatures(string[] args)
    {
        BenchmarkFeatures features = BenchmarkFeatures.None;

        if (args.HasArg("--memory"))
        {
            features |= BenchmarkFeatures.Memory;
        }

        if (args.HasArg("--disassembly", "--disasm"))
        {
            features |= BenchmarkFeatures.Disassembly;
        }

        if (args.HasArg("--long-run"))
        {
            features |= BenchmarkFeatures.LongRun;
        }

        if (args.HasArg("--stable"))
        {
            features |= BenchmarkFeatures.Stable;
        }

        if (args.HasArg("--plotting", "--plot"))
        {
            features |= BenchmarkFeatures.Plotting;
        }



        if (args.HasArg("--deep"))
        {
            features |= BenchmarkFeatures.Memory |
                        BenchmarkFeatures.Disassembly |
                        BenchmarkFeatures.LongRun |
                        BenchmarkFeatures.Stable |
                        BenchmarkFeatures.Plotting;
        }



        return features;
    }

    private static BenchmarkFeatures ApplyFeatureOverrides(BenchmarkFeatures features, string[] args)
    {
        if (args.HasArg("--no-memory"))
        {
            features &= ~BenchmarkFeatures.Memory;
        }

        if (args.HasArg("--no-disassembly", "--no-disasm"))
        {
            features &= ~BenchmarkFeatures.Disassembly;
        }

        if (args.HasArg("--no-long-run"))
        {
            features &= ~BenchmarkFeatures.LongRun;
        }

        if (args.HasArg("--no-stable"))
        {
            features &= ~BenchmarkFeatures.Stable;
        }

        if (args.HasArg("--no-plotting", "--no-plot"))
        {
            features &= ~BenchmarkFeatures.Plotting;
        }

        return features;
    }

    private static BenchmarkCaseSelection ResolveBenchmarkCaseSelection(string[] args)
    {
        return new BenchmarkCaseSelection(
            ParseFilterPatterns(args),
            ParseCategories(args));
    }

    private static string[] ParseFilterPatterns(string[] args)
    {
        var filterPatterns = args.GetArgValues("--filter");

        if (filterPatterns.Length == 1 &&
            filterPatterns[0] == "*")
        {
            return [];
        }

        return filterPatterns;
    }

    private static string[] ParseCategories(string[] args)
    {
        return args.GetArgValues("--category");
    }

    private static IConfig BuildConfig(
        IConfig baseConfig,
        List<Job> effectiveJobs,
        BenchmarkCaseSelection benchmarkCaseSelection,
        BenchmarkExecutionGroupKey benchmarkExecutionGroupKey)
    {
        ManualConfig baseManualConfig = ManualConfig.Create(baseConfig)
                                                    .WithUnionRule(ConfigUnionRule.AlwaysUseLocal);

        baseManualConfig = ApplyBenchmarkCaseSelectionFilters(
            baseManualConfig,
            benchmarkCaseSelection);

        foreach (var job in effectiveJobs)
        {
            baseManualConfig = baseManualConfig.AddJob(job);
        }

        IConfig config = ApplyDiagnosers(
            benchmarkExecutionGroupKey,
            baseManualConfig);

        config = ApplyExporters(
            config,
            benchmarkExecutionGroupKey);

        return config;
    }

    private static ManualConfig ApplyBenchmarkCaseSelectionFilters(
        ManualConfig config,
        BenchmarkCaseSelection benchmarkCaseSelection)
    {
        if (benchmarkCaseSelection.HasFilterPatterns)
        {
            config = config.AddFilter(
                new GlobFilter(benchmarkCaseSelection.FilterPatterns));
        }

        if (benchmarkCaseSelection.HasCategories)
        {
            config = config.AddFilter(
                new AnyCategoriesFilter(benchmarkCaseSelection.Categories));
        }

        return config;
    }

    private static IConfig ApplyDiagnosers(
        BenchmarkExecutionGroupKey benchmarkExecutionGroupKey,
        IConfig config)
    {
        if (HasMemoryFeature(benchmarkExecutionGroupKey.EffectiveFeatures) &&
            !benchmarkExecutionGroupKey.HasMemoryDiagnoserAttribute)
        {
            config = config.AddDiagnoser(CreateMemoryDiagnoser());
        }

        if (HasDisassemblyFeature(benchmarkExecutionGroupKey.EffectiveFeatures) &&
            !benchmarkExecutionGroupKey.HasDisassemblyDiagnoserAttribute)
        {
            config = config.AddDiagnoser(CreateDisassemblyDiagnoser());
        }

        return config;
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IDiagnoser CreateMemoryDiagnoser()
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    {
        return MemoryDiagnoser.Default;
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IDiagnoser CreateDisassemblyDiagnoser()
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    {
        return new DisassemblyDiagnoser(
            new DisassemblyDiagnoserConfig(
                maxDepth: DisassemblyDiagnoser_MaxDepth,
                printSource: true,
                printInstructionAddresses: false,
                exportHtml: true,
                exportCombinedDisassemblyReport: true,
                exportDiff: true));
    }

    private static IConfig ApplyExporters(
        IConfig config,
        BenchmarkExecutionGroupKey benchmarkExecutionGroupKey)
    {
        var exporterDefinitions = BuildEffectiveExporterDefinitions(benchmarkExecutionGroupKey);

        foreach (var exporterDefinition in exporterDefinitions)
        {
            config = config.AddExporter(exporterDefinition.Exporter);
        }

        return config;
    }

    private static List<ExporterDefinition> BuildEffectiveExporterDefinitions(
        BenchmarkExecutionGroupKey benchmarkExecutionGroupKey)
    {
        var exporterDefinitions = new List<ExporterDefinition>();

        if (HasPlottingFeature(benchmarkExecutionGroupKey.EffectiveFeatures))
        {
            exporterDefinitions.Add(CreatePlottingExporter());
        }

        return exporterDefinitions;
    }

    private static ExporterDefinition CreatePlottingExporter()
    {
        return new ExporterDefinition(
            nameof(RPlotExporter),
            RPlotExporter.Default);
    }

    private static List<Job> BuildJobs(
        List<RuntimeJobDefinition> baseRuntimeJobDefinitions,
        List<RuntimeJobDefinition> runtimeJobDefinitions)
    {
        if (runtimeJobDefinitions.Count == 0)
        {
            return [.. baseRuntimeJobDefinitions.Select(r => r.JobFactory())];
        }

        return [.. runtimeJobDefinitions.Select(r => r.JobFactory())];
    }

    /// <summary>
    /// </summary>
    /// <param name="baseRuntimeJobDefinitions"></param>
    /// <param name="runtimeJobDefinitions"></param>
    /// <param name="effectiveFeatures"></param>
    /// <returns></returns>
    /// <remarks>
    ///     REFERENCES:
    ///     -   <see href="https://stackoverflow.com/questions/79631422/remove-or-skip-defaultjob" />
    /// </remarks>
    private static List<Job> BuildEffectiveJobs(
        List<RuntimeJobDefinition> baseRuntimeJobDefinitions,
        List<RuntimeJobDefinition> runtimeJobDefinitions,
        BenchmarkFeatures effectiveFeatures)
    {
        var jobs = BuildJobs(baseRuntimeJobDefinitions, runtimeJobDefinitions);

        var effectiveJobs = new List<Job>(jobs.Count);

        for (var i = 0; i < jobs.Count; i++)
        {
            var effectiveJob = ApplyEffectiveFeatures(jobs[i], effectiveFeatures);

            //
            //  NOTE:   DO NOT set Meta.IsDefault.
            //          It participates in BDN's internal job inference,
            //          and can cause implicit job creation or override explicitly added jobs.
            //
            //effectiveJob.Meta.IsDefault = i == 0;   // INCORRECT
            effectiveJob.Meta.Baseline = i == 0;    // CORRECT

            effectiveJobs.Add(effectiveJob.Freeze());
        }

        return effectiveJobs;
    }

    private static Job ApplyEffectiveFeatures(
        Job job,
        BenchmarkFeatures effectiveFeatures)
    {
        // --- Long runs ---
        if (HasLongRunFeature(effectiveFeatures))
        {
            job = job.WithWarmupCount(Job_LongRun_WarmupCount)
                     .WithIterationCount(Job_LongRun_IterationCount);
        }

        // --- Stability ---
        if (HasStableFeature(effectiveFeatures))
        {
            job = job.WithEnvironmentVariables(
                new("DOTNET_TieredCompilation", "0"),
                new("DOTNET_TieredPGO", "0"),
                new("DOTNET_ReadyToRun", "0"));
        }

        return job;
    }

    private static List<BenchmarkTypePlan> BuildBenchmarkTypePlans(
        List<RuntimeJobDefinition> baseRuntimeJobDefinitions,
        List<RuntimeJobDefinition> runtimeJobDefinitions,
        IConfig baseConfig,
        BenchmarkFeatures baseFeatures,
        List<Type> benchmarkTypes,
        BenchmarkCaseSelection benchmarkCaseSelection)
    {
        var benchmarkTypePlans = new List<BenchmarkTypePlan>(benchmarkTypes.Count);

        foreach (var benchmarkType in benchmarkTypes)
        {
            var benchmarkExecutionGroupKey = ResolveBenchmarkExecutionGroupKey(
                baseFeatures,
                benchmarkType);

            var effectiveJobs = BuildEffectiveJobs(
                baseRuntimeJobDefinitions,
                runtimeJobDefinitions,
                benchmarkExecutionGroupKey.EffectiveFeatures);

            var config = BuildConfig(
                baseConfig,
                effectiveJobs,
                benchmarkCaseSelection,
                benchmarkExecutionGroupKey);

            using var runInfo = BenchmarkConverter.TypeToBenchmarks(
                benchmarkType,
                config);

            if (runInfo.BenchmarksCases.Length == 0)
            {
                continue;
            }

            var benchmarkCaseDisplayInfos =
                runInfo.BenchmarksCases
                       .Select(c => c.DisplayInfo)
                       .OrderBy(s => s, StringComparer.Ordinal)
                       .ToList();

            benchmarkTypePlans.Add(
                new BenchmarkTypePlan(
                    benchmarkType,
                    effectiveJobs,
                    benchmarkExecutionGroupKey,
                    benchmarkCaseDisplayInfos));
        }

        return benchmarkTypePlans;
    }

    private static List<BenchmarkExecutionGroup> BuildBenchmarkExecutionGroups(
        List<BenchmarkTypePlan> benchmarkTypePlans)
    {
        return
        [
            .. benchmarkTypePlans.GroupBy(p => p.BenchmarkExecutionGroupKey)
                                 .OrderBy(g => g.Key.EffectiveFeatures)
                                 .ThenBy(g => g.Key.HasMemoryDiagnoserAttribute)
                                 .ThenBy(g => g.Key.HasDisassemblyDiagnoserAttribute)
                                 .Select(g =>
                                 {
                                     var benchmarkTypePlansInGroup =
                                         g.OrderBy(p => p.BenchmarkType.FullName, Type_FullName_StringComparer)
                                          .ToList();

                                     return new BenchmarkExecutionGroup(
                                         g.Key,
                                         benchmarkTypePlansInGroup);
                                 }),
        ];
    }

    private static BenchmarkExecutionGroupKey ResolveBenchmarkExecutionGroupKey(
        BenchmarkFeatures features,
        Type benchmarkType)
    {
        var effectiveFeatures = features;

        var attribute = benchmarkType.GetAttribute<BenchmarkFeaturesAttribute>();

        if (attribute is not null)
        {
            effectiveFeatures = (effectiveFeatures | attribute.Enable) & ~attribute.Disable;
        }

        var hasMemoryDiagnoserAttribute = HasMemoryDiagnoserAttribute(benchmarkType);
        var hasDisassemblyDiagnoserAttribute = HasDisassemblyDiagnoserAttribute(benchmarkType);

        if (hasMemoryDiagnoserAttribute)
        {
            effectiveFeatures |= BenchmarkFeatures.Memory;
        }

        if (hasDisassemblyDiagnoserAttribute)
        {
            effectiveFeatures |= BenchmarkFeatures.Disassembly;
        }

        return new BenchmarkExecutionGroupKey(
            effectiveFeatures,
            hasMemoryDiagnoserAttribute,
            hasDisassemblyDiagnoserAttribute);
    }

    private static void ShowBenchmarkExecutionPlan(
        string[] args,
        List<RuntimeJobDefinition> baseRuntimeJobDefinitions,
        List<RuntimeJobDefinition> runtimeJobDefinitions,
        BenchmarkFeatures baseFeatures,
        List<Type> benchmarkTypes,
        BenchmarkCaseSelection benchmarkCaseSelection,
        List<BenchmarkExecutionGroup> benchmarkExecutionGroups)
    {
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine("===              BENCHMARK EXECUTION PLAN                ===");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // ---- ARGS ----
        Console.WriteLine("[Args]");

        for (var argsIndex = 0; argsIndex < args.Length; argsIndex++)
        {
            Console.WriteLine($"  [{argsIndex}] \"{args[argsIndex]}\"");
        }

        if (args.Length == 0)
        {
            Console.WriteLine("  (none)");
        }

        // ---- RUNTIMES ARGS ----
        Console.WriteLine();
        Console.WriteLine("[Args.Parsed.Runtimes]");

        var runtimeJobDefinitionIndex = 0;

        foreach (var runtimeJobDefinition in runtimeJobDefinitions)
        {
            Console.WriteLine($"  [{runtimeJobDefinitionIndex}] {runtimeJobDefinition.RuntimeTfm}");

            runtimeJobDefinitionIndex++;
        }

        if (runtimeJobDefinitions.Count == 0)
        {
            Console.WriteLine("  (default: all runtimes)");
        }

        // ---- BASE FEATURES ARGS ----
        Console.WriteLine();
        Console.WriteLine("[Args.Parsed.Features.Base]");
        Console.WriteLine($"  Features.Base: {baseFeatures}");

        // ---- FILTER ARGS ----
        Console.WriteLine();
        Console.WriteLine("[Args.Parsed.Filter]");

        Console.WriteLine(
            benchmarkCaseSelection.FilterPatterns.Length == 0 ?
            "  Filter: (none)" :
            $"  Filter: {string.Join("; ", benchmarkCaseSelection.FilterPatterns)}");

        // ---- CATEGORY FILTER ARGS ----
        Console.WriteLine();
        Console.WriteLine("[Args.Parsed.Category]");

        Console.WriteLine(
            benchmarkCaseSelection.Categories.Length == 0 ?
            "  Category: (none)" :
            $"  Category: {string.Join("; ", benchmarkCaseSelection.Categories)}");

        // ---- BASE JOBS ----
        Console.WriteLine();
        Console.WriteLine("[Jobs.Base]");

        var baseJobs = BuildJobs(baseRuntimeJobDefinitions, runtimeJobDefinitions);

        for (var baseJobIndex = 0; baseJobIndex < baseJobs.Count; baseJobIndex++)
        {
            var baseJob = baseJobs[baseJobIndex];

            Console.WriteLine($"  [{baseJobIndex}]");
            Console.WriteLine($"    Runtime:    {baseJob.Environment.Runtime} ({baseJob.Environment.Runtime!.MsBuildMoniker})");
            Console.WriteLine($"    Baseline:   {baseJobIndex == 0}");
            Console.WriteLine($"    Warmup:     {$"{baseJob.Run?.WarmupCount,6}" ?? "(default)"}");
            Console.WriteLine($"    Iteration:  {$"{baseJob.Run?.IterationCount,6}" ?? "(default)"}");

            if (baseJob.Environment?.EnvironmentVariables?.Count > 0)
            {
                Console.WriteLine($"    Environment Variables:");

                foreach (var environmentVariable in baseJob.Environment.EnvironmentVariables)
                {
                    Console.WriteLine($"      {environmentVariable.Key}={environmentVariable.Value}");
                }
            }
        }

        if (baseJobs.Count == 0)
        {
            Console.WriteLine("  (no base jobs)");
        }

        // ---- BASE FEATURES ----
        Console.WriteLine();
        Console.WriteLine("[Features.Base]");

#if NET5_0_OR_GREATER
        var baseFeatureValues = Enum.GetValues<BenchmarkFeatures>()
                                    .Where(f => f != BenchmarkFeatures.None)
                                    .ToList().AsReadOnly();
#else
        var baseFeatureValues = Array.AsReadOnly(((BenchmarkFeatures[]) Enum.GetValues(typeof(BenchmarkFeatures))).Where(f => f != BenchmarkFeatures.None).ToArray());
#endif

        var baseFeatureIndex = 0;

        foreach (var baseFeature in baseFeatureValues)
        {
            Console.WriteLine($"  [{baseFeatureIndex}] {$"{baseFeature}:",-16} {((baseFeatures & baseFeature) != 0 ? "enabled" : "disabled")}");

            baseFeatureIndex++;
        }

        // ---- SUMMARY ----
        Console.WriteLine();
        Console.WriteLine("[Execution.Summary]");

        var runtimesSummary = string.Join(", ", baseJobs.Select(job => $"{job.Environment.Runtime} ({job.Environment.Runtime!.MsBuildMoniker})"));
        var baseFeaturesSummary = baseFeatures.ToString();
        var selectedBenchmarkTypeCount = benchmarkExecutionGroups.Sum(g => g.BenchmarkTypeCount);
        var selectedBenchmarkCaseCount = benchmarkExecutionGroups.Sum(g => g.BenchmarkTypePlans.Sum(p => p.BenchmarkCaseCount));

        Console.WriteLine($"  Runtimes:         {runtimesSummary}");
        Console.WriteLine($"  Features.Base:    {baseFeaturesSummary}");
        Console.WriteLine($"  BenchmarkTypes.Total:           {benchmarkTypes.Count,6}");
        Console.WriteLine($"  BenchmarkTypes.Selected.Total:  {selectedBenchmarkTypeCount,6}");
        Console.WriteLine($"  BenchmarkCases.Selected.Total:  {selectedBenchmarkCaseCount,6}");

        // ---- GROUPS ----
        Console.WriteLine();
        Console.WriteLine("[Execution.Groups]");
        Console.WriteLine($"  Groups.Total:                   {benchmarkExecutionGroups.Count,6}");
        Console.WriteLine($"  Groups.BenchmarkTypes.Total:    {benchmarkExecutionGroups.Sum(g => g.BenchmarkTypeCount),6}");
        Console.WriteLine($"  Groups.BenchmarkCases.Total:    {benchmarkExecutionGroups.Sum(g => g.BenchmarkCaseCount),6}");

        var benchmarkExecutionGroupIndex = 0;

        foreach (var benchmarkExecutionGroup in benchmarkExecutionGroups)
        {
            var benchmarkExecutionGroupKey = benchmarkExecutionGroup.BenchmarkExecutionGroupKey;
            var effectiveJobs = benchmarkExecutionGroup.BenchmarkTypePlans[0].EffectiveJobs;
            var hasConfigAttribute = benchmarkExecutionGroup.BenchmarkTypePlans.Any(p => HasConfigAttribute(p.BenchmarkType));
            var hasEffectiveMemoryFeature = HasMemoryFeature(benchmarkExecutionGroupKey.EffectiveFeatures);
            var hasEffectiveDisassemblyFeature = HasDisassemblyFeature(benchmarkExecutionGroupKey.EffectiveFeatures);
            var hasEffectivePlottingFeature = HasPlottingFeature(benchmarkExecutionGroupKey.EffectiveFeatures);
            var exporterDefinitions = BuildEffectiveExporterDefinitions(benchmarkExecutionGroupKey);

            Console.WriteLine($"  [{benchmarkExecutionGroupIndex + 1}]");
            Console.WriteLine($"    {nameof(ConfigAttribute)}:        {(hasConfigAttribute ? "defined" : "-")}");
            Console.WriteLine($"    Features.Effective:     {benchmarkExecutionGroupKey.EffectiveFeatures}");
            Console.WriteLine($"    Exporters.Effective:    {(exporterDefinitions.Count > 0 ? string.Join(", ", exporterDefinitions.Select(e => e.Name)) : "-")}");
            Console.WriteLine($"    Jobs.Effective:");

            for (var effectiveJobIndex = 0; effectiveJobIndex < effectiveJobs.Count; effectiveJobIndex++)
            {
                var effectiveJob = effectiveJobs[effectiveJobIndex];

                Console.WriteLine($"      [{effectiveJobIndex}]");
                Console.WriteLine($"        Runtime:    {effectiveJob.Environment.Runtime} ({effectiveJob.Environment.Runtime!.MsBuildMoniker})");
                Console.WriteLine($"        Baseline:   {effectiveJob.Meta.Baseline}");
                Console.WriteLine($"        Warmup:     {$"{effectiveJob.Run?.WarmupCount,6}" ?? "(default)"}");
                Console.WriteLine($"        Iteration:  {$"{effectiveJob.Run?.IterationCount,6}" ?? "(default)"}");

                if (effectiveJob.Environment?.EnvironmentVariables?.Count > 0)
                {
                    Console.WriteLine($"        Environment Variables:");

                    foreach (var environmentVariable in effectiveJob.Environment.EnvironmentVariables)
                    {
                        Console.WriteLine($"          {environmentVariable.Key}={environmentVariable.Value}");
                    }
                }
            }

            Console.WriteLine($"    Group[{benchmarkExecutionGroupIndex + 1}].BenchmarkTypes.Subtotal:    {benchmarkExecutionGroup.BenchmarkTypeCount,6}");
            Console.WriteLine($"    Group[{benchmarkExecutionGroupIndex + 1}].BenchmarkCases.Subtotal:    {benchmarkExecutionGroup.BenchmarkCaseCount,6}");

            var benchmarkTypePlanIndex = 0;

            foreach (var benchmarkTypePlan in benchmarkExecutionGroup.BenchmarkTypePlans)
            {
                var benchmarkType = benchmarkTypePlan.BenchmarkType;

                Console.Write($"    [{benchmarkExecutionGroupIndex + 1}.{benchmarkTypePlanIndex + 1}] ");
                ConsoleHelper.Write(benchmarkType.Name, ConsoleColor.DarkCyan);
                Console.WriteLine($" (in {benchmarkType.Namespace})");
                Console.WriteLine($"      BenchmarkCases.Selected: {benchmarkTypePlan.BenchmarkCaseCount}");

                if (hasEffectiveMemoryFeature &&
                    !HasMemoryFeature(baseFeatures))
                {
                    var attributeName = HasMemoryDiagnoserAttribute(benchmarkType) ?
                                        nameof(MemoryDiagnoserAttribute) :
                                        nameof(BenchmarkFeaturesAttribute);

                    Console.WriteLine($"      '{attributeName}' overrides disabled Memory base feature.");
                }

                if (hasEffectiveDisassemblyFeature &&
                    !HasDisassemblyFeature(baseFeatures))
                {
                    var attributeName = HasDisassemblyDiagnoserAttribute(benchmarkType) ?
                                        nameof(DisassemblyDiagnoserAttribute) :
                                        nameof(BenchmarkFeaturesAttribute);

                    Console.WriteLine($"      '{attributeName}' overrides disabled Disassembly base feature.");
                }

                if (hasEffectivePlottingFeature &&
                    !HasPlottingFeature(baseFeatures))
                {
                    Console.WriteLine($"      '{nameof(BenchmarkFeaturesAttribute)}' overrides disabled Plotting base feature.");
                }

                var benchmarkCaseIndex = 0;

                foreach (var benchmarkCaseDisplayInfo in benchmarkTypePlan.BenchmarkCaseDisplayInfos)
                {
                    Console.Write($"      [{benchmarkExecutionGroupIndex + 1}.{benchmarkTypePlanIndex + 1}.{benchmarkCaseIndex + 1}] ");
                    Console.WriteLine(benchmarkCaseDisplayInfo);

                    benchmarkCaseIndex++;
                }

                benchmarkTypePlanIndex++;
            }

            benchmarkExecutionGroupIndex++;
        }

        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine();
    }

    private static void ExecuteBenchmarks(
        IConfig baseConfig,
        BenchmarkCaseSelection benchmarkCaseSelection,
        List<BenchmarkExecutionGroup> benchmarkExecutionGroups)
    {
        foreach (var benchmarkExecutionGroup in benchmarkExecutionGroups)
        {
            var benchmarkTypes =
                benchmarkExecutionGroup.BenchmarkTypePlans
                                       .Select(p => p.BenchmarkType)
                                       .ToArray();

            var config = BuildConfig(
                baseConfig,
                benchmarkExecutionGroup.BenchmarkTypePlans[0].EffectiveJobs,
                benchmarkCaseSelection,
                benchmarkExecutionGroup.BenchmarkExecutionGroupKey);

            BenchmarkRunner.Run(benchmarkTypes, config);
        }
    }

    private static bool IsPreviewMode(string[] args)
    {
        return args.HasArg("--preview", "--dry-run");
    }

    public static bool HasMemoryFeature(BenchmarkFeatures features)
    {
        return (features & BenchmarkFeatures.Memory) != 0;
    }

    public static bool HasDisassemblyFeature(BenchmarkFeatures features)
    {
        return (features & BenchmarkFeatures.Disassembly) != 0;
    }

    public static bool HasLongRunFeature(BenchmarkFeatures features)
    {
        return (features & BenchmarkFeatures.LongRun) != 0;
    }

    public static bool HasStableFeature(BenchmarkFeatures features)
    {
        return (features & BenchmarkFeatures.Stable) != 0;
    }

    public static bool HasPlottingFeature(BenchmarkFeatures features)
    {
        return (features & BenchmarkFeatures.Plotting) != 0;
    }

    public static bool IsBenchmarkType(Type benchmarkType)
    {
        return benchmarkType.GetMethods().Any(m => m.HasAttribute<BenchmarkAttribute>());
    }

    public static bool HasConfigAttribute(Type benchmarkType)
    {
        return benchmarkType.HasAttribute<ConfigAttribute>();
    }

    public static bool HasBenchmarkFeaturesAttribute(Type benchmarkType)
    {
        return benchmarkType.HasAttribute<BenchmarkFeaturesAttribute>();
    }

    public static bool HasMemoryDiagnoserAttribute(Type benchmarkType)
    {
        return benchmarkType.HasAttribute<MemoryDiagnoserAttribute>();
    }

    public static bool HasDisassemblyDiagnoserAttribute(Type benchmarkType)
    {
        return benchmarkType.HasAttribute<DisassemblyDiagnoserAttribute>();
    }
}



internal static class ReflectionExtensions
{
    public static bool HasAttribute<T>(this Type type, bool inherit = false)
        where T : Attribute
        =>
        type.IsDefined(typeof(T), inherit);

    public static bool HasAttribute<T>(this MemberInfo member, bool inherit = false)
        where T : Attribute
        =>
        member.IsDefined(typeof(T), inherit);

    public static IEnumerable<T> GetAttributes<T>(this Type type, bool inherit = false)
        where T : Attribute
        =>
        type.GetCustomAttributes<T>(inherit);

    public static T? GetAttribute<T>(this Type type, bool inherit = false)
        where T : Attribute
        =>
        type.GetCustomAttribute<T>(inherit);

    public static IEnumerable<T> GetAttributes<T>(this MemberInfo member, bool inherit = false)
        where T : Attribute
        =>
        member.GetCustomAttributes<T>(inherit);

    public static T? GetAttribute<T>(this MemberInfo member, bool inherit = false)
        where T : Attribute
        =>
        member.GetCustomAttribute<T>(inherit);
}

internal static class StringExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty([NotNullWhen(false)] this string? str) =>
        str is null || str.Length == 0;

#if NETFRAMEWORK
    public static bool Contains(this string str, char value, StringComparison comparisonType) =>
        str.IndexOf(value.ToString(), comparisonType) >= 0;

    public static bool StartsWith(this string str, char value) =>
        str.StartsWith(value.ToString(), StringComparison.Ordinal);

    public static bool EndsWith(this string str, char value) =>
        str.EndsWith(value.ToString(), StringComparison.Ordinal);

    public static string[] Split(this string str, char separator, StringSplitOptions options = StringSplitOptions.None) =>
        str.Split([separator], options);

    public static string[] Split(this string str, char separator, int count, StringSplitOptions options = StringSplitOptions.None) =>
        str.Split([separator], count, options);
#endif

    public static string[] SplitBySemicolon(this string str) =>
        [
            .. str.Split(';', StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => s.Trim())
                  .Where(s => s.Length != 0)
        ];
}

internal static class StringArgsExtensions
{
    public static bool HasArg(this string[] args, params string[] names)
    {
        foreach (var arg in args)
        {
            foreach (var name in names)
            {
                if (arg.Equals(name, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static string? GetArgValue(this string[] args, string name)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith(name + "=", StringComparison.Ordinal))
            {
                var value = arg.Split('=', 2)[1];

                if (value.Length == 0)
                {
                    throw new ArgumentException($"Missing value for '{name}'.");
                }

                return value;
            }

            if (arg == name)
            {
                if (i + 1 >= args.Length ||
                    args[i + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Missing value for '{name}'.");
                }

                return args[i + 1];
            }
        }

        return null;
    }

    public static string[] GetArgValues(this string[] args, params string[] names)
    {
        var values = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            var matchedName =
                names.FirstOrDefault(name =>
                                     arg.Equals(name, StringComparison.Ordinal) ||
                                     arg.StartsWith(name + "=", StringComparison.Ordinal));

            if (matchedName is null)
            {
                continue;
            }

            if (arg.StartsWith(matchedName + "=", StringComparison.Ordinal))
            {
                var value = arg[(matchedName.Length + 1)..];

                AddListValue(values, value);

                continue;
            }

            var hasValue = false;

            for (int j = i + 1; j < args.Length; j++)
            {
                var value = args[j];

                if (value.StartsWith("--", StringComparison.Ordinal))
                {
                    break;
                }

                AddListValue(values, value);

                hasValue = true;
            }

            if (!hasValue)
            {
                throw new ArgumentException($"Missing value for '{matchedName}'.");
            }
        }

        return [.. values.Distinct(StringComparer.Ordinal)];
    }

    private static void AddListValue(List<string> values, string value)
    {
        var splitValues = value.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var splitValue in splitValues)
        {
            var normalizedValue = splitValue.Trim();

            if (normalizedValue.Length == 0)
            {
                continue;
            }

            values.Add(normalizedValue);
        }
    }
}

internal static class ConsoleHelper
{
    public static void Write(string value, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;

        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ForegroundColor = originalColor;
    }
}
