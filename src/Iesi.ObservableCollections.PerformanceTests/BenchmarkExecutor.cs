using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Iesi.Collections.Generic.PerformanceTests;

public class BenchmarkExecutor
{
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
        Job baseJob,
        IConfig baseConfig)
    {
        Type Type = type;
        List<Type> AllTypes = [.. Type.Assembly.GetTypes().OrderBy(t => t.FullName, Type_FullName_StringComparer)];
        List<Type> AllBenchmarkTypes = [.. AllTypes.Where(IsBenchmarkType).OrderBy(t => t.FullName, Type_FullName_StringComparer)];
        List<RuntimeJob> BaseRuntimeJobs = CreateRuntimeJobs(BaseRuntimes, baseJob);
        Dictionary<string, RuntimeJob> BaseRuntimeJobsByRuntimeTfm = BaseRuntimeJobs.ToDictionary(rj => rj.RuntimeTfm, StringComparer.Ordinal);



        var runtimeJobs = ParseRuntimeJobs(args, BaseRuntimeJobsByRuntimeTfm);

        var baseFeatures = ResolveBaseFeatures(args);



        var jobs = BuildJobs(runtimeJobs, BaseRuntimeJobs);

        SetBaselineJob(jobs);



        var benchmarks = FilterBenchmarks(args, AllBenchmarkTypes);

        benchmarks = ApplyCategoryFilter(args, benchmarks);

        if (benchmarks.Count == 0)
        {
            Console.WriteLine("No benchmarks matched current filter and/or category.");

            return;
        }



        var benchmarksBySignature = GroupBenchmarksByExecutionSignature(baseFeatures, benchmarks);



        // ---- ANALYSIS ----

        ShowBenchmarksExecutionPlan(args, runtimeJobs, jobs, baseFeatures, benchmarks, benchmarksBySignature);



        if (IsPreviewMode(args))
        {
            return;
        }



        // ---- EXECUTION ----

        ExecuteBenchmarks(jobs, baseConfig, benchmarksBySignature);
    }



    private static List<RuntimeJob> CreateRuntimeJobs(List<Runtime> baseRuntimes, Job baseJob)
    {
        if (baseRuntimes.Count == 0)
        {
            throw new InvalidOperationException("No runtimes defined.");
        }

        var runtimeJobs =
            baseRuntimes.Select(r =>
                                new RuntimeJob(
                                    r.MsBuildMoniker,
                                    () => baseJob.WithRuntime(r)))

                        .ToList();

        return runtimeJobs;
    }

    private static List<RuntimeJob> ParseRuntimeJobs(
        string[] args,
        Dictionary<string, RuntimeJob> baseRuntimeJobsByRuntimeTfm)
    {
        var values = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (!arg.StartsWith("--runtimes", StringComparison.Ordinal))
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

        values = [.. values.Distinct()];

        if (values.Count == 0)
        {
            return [];
        }

        var runtimeJobs = new List<RuntimeJob>(values.Count);

        foreach (var runtimeTfm in values)
        {
            if (!baseRuntimeJobsByRuntimeTfm.TryGetValue(runtimeTfm, out var runtimeJob))
            {
                throw new ArgumentException($"Unknown runtime TFM: '{runtimeTfm}'. Runtime must exactly match TFM (Target Framework Moniker).");
            }

            runtimeJobs.Add(runtimeJob);
        }

        return runtimeJobs;
    }

    private static BenchmarkFeatures ResolveBaseFeatures(string[] args)
    {
        var baseFeatures = BenchmarkFeatures.None;

        baseFeatures = baseFeatures |
                       ParseCliProfileFeatures(args) |
                       ParseCliFeatures(args);

        baseFeatures = ApplyBaseFeatureOverrides(args, baseFeatures);

        return baseFeatures;
    }

    private static BenchmarkFeatures ApplyBaseFeatureOverrides(string[] args, BenchmarkFeatures baseFeatures)
    {
        if (args.HasArg("--no-memory"))
        {
            baseFeatures &= ~BenchmarkFeatures.Memory;
        }

        if (args.HasArg("--no-disassembly", "--no-disasm"))
        {
            baseFeatures &= ~BenchmarkFeatures.Disassembly;
        }

        return baseFeatures;
    }

    private static BenchmarkFeatures ParseCliProfileFeatures(string[] args)
    {
        string? value = null;

        for (int i = 0; i < args.Length; i++)
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

            _ => BenchmarkFeatures.None
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

        if (args.HasArg("--deep"))
        {
            features |= BenchmarkFeatures.Disassembly |
                        BenchmarkFeatures.Stable;
        }

        return features;
    }

    private static List<Job> BuildJobs(List<RuntimeJob> runtimeJobs, List<RuntimeJob> baseRuntimeJobs)
    {
        if (runtimeJobs.Count == 0)
        {
            return [.. baseRuntimeJobs.Select(r => r.JobBuilder())];
        }

        return [.. runtimeJobs.Select(r => r.JobBuilder())];
    }

    /// <summary>
    /// </summary>
    /// <param name="jobs"></param>
    /// <remarks>
    ///     REFERENCES:
    ///     -   <see href="https://stackoverflow.com/questions/79631422/remove-or-skip-defaultjob" />
    /// </remarks>
    private static void SetBaselineJob(List<Job> jobs)
    {
        for (int i = 0; i < jobs.Count; i++)
        {
            var isPrimary = i == 0;

            // DO NOT set Meta.IsDefault.
            // It participates in BDN's internal job inference,
            // and can cause implicit job creation or override explicitly added jobs.
            //jobs[i].Meta.IsDefault = isPrimary;
            jobs[i].Meta.Baseline = isPrimary;
        }
    }

    private static List<Type> FilterBenchmarks(string[] args, List<Type> benchmarks)
    {
        var filter = args.GetArgValue("--filter");

        if (filter.IsEmpty() || filter == "*")
        {
            return [.. benchmarks];
        }

        var patterns = filter.SplitBySemicolon();

        return [.. benchmarks.Where(b => patterns.Any(p => MatchesFilter(b, p)))];
    }

    private static bool MatchesFilter(Type benchmark, string pattern)
    {
        string name = benchmark.FullName!;

        if (pattern.StartsWith('*') && pattern.EndsWith('*'))
        {
            return name.Contains(pattern.Trim('*'), StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.StartsWith('*'))
        {
            return name.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.EndsWith('*'))
        {
            return name.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);
        }

        return name.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static List<Type> ApplyCategoryFilter(string[] args, List<Type> benchmarks)
    {
        var category = args.GetArgValue("--category");

        if (category.IsEmpty())
        {
            return benchmarks;
        }

        var categories = category.SplitBySemicolon();

        return [.. benchmarks.Where(b => categories.Any(c => HasCategory(b, c)))];
    }

    private static bool HasCategory(Type benchmark, string category)
    {
        // --- Type-level ---
        if (benchmark.GetAttributes<BenchmarkCategoryAttribute>()
                     .Any(a => a.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)))
        {
            return true;
        }

        // --- Method-level ---
        return benchmark.GetMethods()
                        .Where(m => m.HasAttribute<BenchmarkAttribute>())
                        .SelectMany(m => m.GetAttributes<BenchmarkCategoryAttribute>())
                        .Any(a => a.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));
    }

    private static Dictionary<ExecutionSignature, List<Type>> GroupBenchmarksByExecutionSignature(
        BenchmarkFeatures baseFeatures,
        List<Type> benchmarks)
    {
        return benchmarks.GroupBy(b => ResolveSignature(baseFeatures, b))
                         .OrderBy(g => g.Key.EffectiveFeatures)
                         .ThenBy(g => g.Key.ConfigIdentity)
                         .ToDictionary(
                             g => g.Key,
                             g => g.OrderBy(t => t.FullName, Type_FullName_StringComparer)
                                   .ToList());
    }

    private static ExecutionSignature ResolveSignature(
        BenchmarkFeatures baseFeatures,
        Type benchmark)
    {
        var features = baseFeatures;

        var attribute = benchmark.GetAttribute<BenchmarkFeaturesAttribute>();

        if (attribute is not null)
        {
            features = (features | attribute.Enable) & ~attribute.Disable;
        }

        if (HasMemoryDiagnoserAttribute(benchmark))
        {
            features |= BenchmarkFeatures.Memory;
        }

        if (HasDisassemblyDiagnoserAttribute(benchmark))
        {
            features |= BenchmarkFeatures.Disassembly;
        }

        int configIdentity = GetConfigIdentity(benchmark);

        return new ExecutionSignature(
            features,
            configIdentity);
    }

    private static void ExecuteBenchmarks(
        List<Job> jobs,
        IConfig baseConfig,
        Dictionary<ExecutionSignature, List<Type>> benchmarksBySignature)
    {
        foreach (var group in benchmarksBySignature)
        {
            var signature = group.Key;
            var groupedBenchmarks = group.Value;

            // Representative benchmark (guaranteed equivalent).
            var benchmark = groupedBenchmarks[0];

            var config = BuildConfig(
                jobs,
                signature.EffectiveFeatures,
                baseConfig,
                benchmark);

            BenchmarkRunner.Run([.. groupedBenchmarks], config);
        }
    }

    private static IConfig BuildConfig(
        List<Job> jobs,
        BenchmarkFeatures effectiveFeatures,
        IConfig baseConfig,
        Type benchmark)
    {
        // ---- GLOBAL BASE CONFIG ----
        ManualConfig baseManualConfig = ManualConfig.Create(baseConfig)
                                                    .WithUnionRule(ConfigUnionRule.AlwaysUseLocal);

        // ---- LOCAL (CLONED) BASE (ATTRIBUTE) CONFIG ----
        IConfig? benchmarkBaseConfig = GetConfig(benchmark);

        if (benchmarkBaseConfig is not null)
        {
            baseManualConfig.Add(benchmarkBaseConfig);
        }

        // ---- JOBS ----
        foreach (var job in jobs)
        {
            var jobWithFeatures = ApplyFeatures(effectiveFeatures, job).Freeze();

            baseManualConfig = baseManualConfig.AddJob(jobWithFeatures);
        }

        // ---- DIAGNOSERS ----
        IConfig benchmarkConfig = ApplyDiagnosers(effectiveFeatures, baseManualConfig, benchmark);

        return benchmarkConfig;
    }

    private static IConfig? GetConfig(Type benchmark)
    {
        var attribute = benchmark.GetAttribute<ConfigAttribute>();

        if (attribute?.Config is null)
        {
            return null;
        }

        // CRITICAL: clone to avoid shared mutation.
        return ManualConfig.Create(attribute.Config);
    }

    private static int GetConfigIdentity(Type benchmark)
    {
        var attribute = benchmark.GetAttribute<ConfigAttribute>();

        return attribute?.Config is not null ?
               GetFilterIdentity(attribute.Config) :
               0;
    }

    private static int GetFilterIdentity(IConfig config)
    {
        //
        //  NOTES:  Config identity is defined solely by filter types.
        //          Filter instance state is intentionally ignored.
        //

        var filters = config.GetFilters().ToList();
        var filterTypes = filters.Select(f => f.GetType())
                                 .OrderBy(t => t.FullName, Type_FullName_StringComparer)
                                 .ToList();

        int hash = 47;

        foreach (var filterType in filterTypes)
        {
            hash = (hash * 101) ^ RuntimeHelpers.GetHashCode(filterType);
        }

        return hash;
    }

    private static Job ApplyFeatures(
        BenchmarkFeatures effectiveFeatures,
        Job job)
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
                new("DOTNET_TieredPGO", "1"),
                new("DOTNET_ReadyToRun", "0"));
        }

        return job;
    }

    private static IConfig ApplyDiagnosers(
        BenchmarkFeatures effectiveFeatures,
        IConfig config,
        Type benchmark)
    {
        if (HasMemoryFeature(effectiveFeatures) &&
            !HasMemoryDiagnoserAttribute(benchmark))
        {
            config = config.AddDiagnoser(MemoryDiagnoser.Default);
        }

        if (HasDisassemblyFeature(effectiveFeatures) &&
            !HasDisassemblyDiagnoserAttribute(benchmark))
        {
            config = config.AddDiagnoser(
                new DisassemblyDiagnoser(
                    new DisassemblyDiagnoserConfig(
                        maxDepth: DisassemblyDiagnoser_MaxDepth,
                        exportCombinedDisassemblyReport: true,
                        printSource: true,
                        printInstructionAddresses: true)));
        }

        return config;
    }

    private static void ShowBenchmarksExecutionPlan(
        string[] args,
        List<RuntimeJob> runtimeJobs,
        List<Job> jobs,
        BenchmarkFeatures baseFeatures,
        List<Type> benchmarks,
        Dictionary<ExecutionSignature, List<Type>> benchmarksBySignature)
    {
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine("===              BENCHMARK EXECUTION PLAN                ===");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        int i = 0;

        // ---- ARGS ----
        Console.WriteLine("[Args]");

        for (i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"  [{i}] \"{args[i]}\"");
        }

        if (args.Length == 0)
        {
            Console.WriteLine("  (none)");
        }

        // ---- RUNTIMES ARGS ----
        Console.WriteLine();
        Console.WriteLine("[Args.Parsed.Runtimes]");

        i = 0;

        foreach (var runtimeJob in runtimeJobs)
        {
            Console.WriteLine($"  [{i}] {runtimeJob.RuntimeTfm}");

            i++;
        }

        if (runtimeJobs.Count == 0)
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

        var filter = args.GetArgValue("--filter");

        Console.WriteLine($"  Filter: {filter ?? "(none)"}");

        // ---- CATEGORY FILTER ARGS ----
        Console.WriteLine();
        Console.WriteLine("[Args.Parsed.Category]");

        var category = args.GetArgValue("--category");

        Console.WriteLine($"  Category: {category ?? "(none)"}");

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

        i = 0;

        foreach (var feature in baseFeatureValues)
        {
            Console.WriteLine($"  [{i}] {$"{feature}:",-16} {((baseFeatures & feature) != 0 ? "enabled" : "disabled")}");

            i++;
        }

        // ---- JOBS ----
        Console.WriteLine();
        Console.WriteLine("[Jobs]");

        for (i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];

            Console.WriteLine($"  [{i}]");
            Console.WriteLine($"    Runtime:   {job.Environment.Runtime} ({job.Environment.Runtime!.MsBuildMoniker})");
            Console.WriteLine($"    Baseline:  {job.Meta.Baseline}");
            Console.WriteLine($"    Warmup:    {job.Run?.WarmupCount.ToString() ?? "(default)"}");
            Console.WriteLine($"    Iteration: {job.Run?.IterationCount.ToString() ?? "(default)"}");

            if (job.Environment?.EnvironmentVariables?.Count > 0)
            {
                Console.WriteLine($"    Environment Variables:");

                foreach (var environmentVariable in job.Environment.EnvironmentVariables)
                {
                    Console.WriteLine($"      {environmentVariable.Key}={environmentVariable.Value}");
                }
            }
        }

        if (jobs.Count == 0)
        {
            Console.WriteLine("  (no jobs)");
        }

        // ---- SUMMARY ----
        Console.WriteLine();
        Console.WriteLine("[Execution.Summary]");

        var runtimesSummary = string.Join(", ", jobs.Select(j => $"{j.Environment.Runtime} ({j.Environment.Runtime!.MsBuildMoniker})"));
        var baseFeaturesSummary = baseFeatures.ToString();

        Console.WriteLine($"  Runtimes:         {runtimesSummary}");
        Console.WriteLine($"  Features.Base:    {baseFeaturesSummary}");
        Console.WriteLine($"  Benchmarks.Total: {benchmarks.Count}");

        // ---- GROUPS ----
        Console.WriteLine();
        Console.WriteLine("[Execution.Groups]");
        Console.WriteLine($"  Groups.Total:             {benchmarksBySignature.Count}");
        Console.WriteLine($"  Groups.Benchmarks.Total:  {benchmarksBySignature.Sum(g => g.Value.Count)}");

        i = 0;

        foreach (var group in benchmarksBySignature)
        {
            var signature = group.Key;

            var hasEffectiveMemoryFeature = HasMemoryFeature(signature.EffectiveFeatures);
            var hasEffectiveDisassemblyFeature = HasDisassemblyFeature(signature.EffectiveFeatures);

            Console.WriteLine($"  [{i + 1}]");
            Console.WriteLine($"    Features.Effective:         {signature.EffectiveFeatures}");
            Console.WriteLine($"    ConfigIdentity:             {signature.ConfigIdentity}{(HasConfigAttribute(group.Value[0]) ? $" (has '{nameof(ConfigAttribute)}' defined)" : "")}");
            Console.WriteLine($"    Groups.Benchmarks.Subtotal: {group.Value.Count}");

            int j = 0;

            foreach (var benchmark in group.Value)
            {
                //Console.WriteLine($"    [{i + 1}.{j + 1}] {benchmark.Name} (in {benchmark.Namespace})");
                Console.Write($"    [{i + 1}.{j + 1}] ");
                ConsoleHelper.Write(benchmark.Name, ConsoleColor.DarkCyan);
                Console.WriteLine($" (in {benchmark.Namespace})");

                if (hasEffectiveMemoryFeature &&
                    !HasMemoryFeature(baseFeatures))
                {
                    var attributeName = HasMemoryDiagnoserAttribute(benchmark) ?
                                        nameof(MemoryDiagnoserAttribute) :
                                        nameof(BenchmarkFeaturesAttribute);

                    Console.WriteLine($"      '{attributeName}' overrides disabled Memory base feature.");
                }

                if (hasEffectiveDisassemblyFeature &&
                    !HasDisassemblyFeature(baseFeatures))
                {
                    var attributeName = HasDisassemblyDiagnoserAttribute(benchmark) ?
                                        nameof(DisassemblyDiagnoserAttribute) :
                                        nameof(BenchmarkFeaturesAttribute);

                    Console.WriteLine($"      '{attributeName}' overrides disabled Disassembly base feature.");
                }

                j++;
            }

            i++;
        }

        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine();
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

    public static bool IsBenchmarkType(Type benchmark)
    {
        return benchmark.GetMethods().Any(m => m.HasAttribute<BenchmarkAttribute>());
    }

    public static bool HasConfigAttribute(Type benchmark)
    {
        return benchmark.HasAttribute<ConfigAttribute>();
    }

    public static bool HasBenchmarkFeaturesAttribute(Type benchmark)
    {
        return benchmark.HasAttribute<BenchmarkFeaturesAttribute>();
    }

    public static bool HasMemoryDiagnoserAttribute(Type benchmark)
    {
        return benchmark.HasAttribute<MemoryDiagnoserAttribute>();
    }

    public static bool HasDisassemblyDiagnoserAttribute(Type benchmark)
    {
        return benchmark.HasAttribute<DisassemblyDiagnoserAttribute>();
    }
}

[Flags]
public enum BenchmarkFeatures
{
    None = 0,
    Memory = 1 << 0,
    Disassembly = 1 << 1,
    LongRun = 1 << 2,
    Stable = 1 << 3
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

internal sealed record RuntimeJob(
    string RuntimeTfm,
    Func<Job> JobBuilder);

internal sealed record ExecutionSignature(
    BenchmarkFeatures EffectiveFeatures,
    int ConfigIdentity);



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
        str.Split(';', StringSplitOptions.RemoveEmptyEntries);

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
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith(name + "=", StringComparison.Ordinal))
            {
                return arg.Split('=', 2)[1];
            }

            if (arg == name && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return null;
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
