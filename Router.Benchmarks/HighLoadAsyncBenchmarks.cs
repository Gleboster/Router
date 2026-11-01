using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace Router.Benchmarks;

/// <summary>
/// Benchmark 5: Асинхронная высокая нагрузка (цель: 5000+ ops/sec).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class HighLoadAsyncBenchmarks : BenchmarkBase
{
    private const int OperationsPerInvoke = 5000;

    private CancellationTokenSource _cts = null!;
    private Random[] _randomGenerators = null!;
    private int _oldMinWorker;
    private int _oldMinIo;

    [Params(1, 2, 4, 8, 16)]
    public int ConcurrentThreads { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        ThreadPool.GetMinThreads(out _oldMinWorker, out _oldMinIo);
        var minThreads = Math.Max(ConcurrentThreads * 2, 32);
        ThreadPool.SetMinThreads(minThreads, minThreads);

        _cts = new CancellationTokenSource();
        SetupRoutes(_ => { });

        _randomGenerators = new Random[ConcurrentThreads];
        for (int i = 0; i < ConcurrentThreads; i++)
            _randomGenerators[i] = new Random(42 + i);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cts.Dispose();
        ThreadPool.SetMinThreads(_oldMinWorker, _oldMinIo);
    }

    /// <summary>
    /// Асинхронная пропускная способность с параллелизмом (цель: 5000+ ops/sec суммарно).
    /// </summary>
    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public Task HighLoad_Async_5000Routes()
    {
        var token = _cts.Token;
        var routes = Routes;
        var perSender = OperationsPerInvoke / ConcurrentThreads;
        var tasks = new Task[ConcurrentThreads];

        for (int i = 0; i < ConcurrentThreads; i++)
        {
            var rnd = _randomGenerators[i];
            tasks[i] = Task.Run(async () =>
            {
                for (int j = 0; j < perSender; j++)
                {
                    await Router.RouteAsync(routes[rnd.Next(0, 100)], token);
                }
            });
        }

        return Task.WhenAll(tasks);
    }
}