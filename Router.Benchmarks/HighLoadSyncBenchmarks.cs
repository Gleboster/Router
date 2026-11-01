using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace Router.Benchmarks;

/// <summary>
/// Benchmark 4: Синхронная высокая нагрузка (цель: 5000+ ops/sec).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class HighLoadSyncBenchmarks : BenchmarkBase
{
    private const int OperationsPerInvoke = 5000;

    [GlobalSetup]
    public void Setup()
    {
        SetupRoutes(_ => { });
    }

    /// <summary>
    /// Синхронная пропускная способность роутинга (цель: 5000+ ops/sec).
    /// </summary>
    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void HighLoad_Sync_5000Routes()
    {
        var routes = Routes;
        for (int i = 0; i < OperationsPerInvoke; i++)
        {
            Router.Route(routes[i % 100]);
        }
    }
}