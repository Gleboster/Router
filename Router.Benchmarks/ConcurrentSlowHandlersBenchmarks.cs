using BenchmarkDotNet.Attributes;

namespace Router.Benchmarks;

/// <summary>
/// Benchmark 3: Одновременная маршрутизация с медленными обработчиками (имитирует операции ввода-вывода).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class ConcurrentSlowHandlersBenchmarks : BenchmarkBase
{
    private CancellationTokenSource _cts = null!;
    private Random[] _randomGenerators = null!;
    private int _oldMinWorker;
    private int _oldMinIo;

    [Params(1, 2, 4, 8, 16)]
    public int ConcurrentThreads { get; set; }

    [Params(10, 20, 50, 100)]
    public int HandlerDurationMs { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        ThreadPool.GetMinThreads(out _oldMinWorker, out _oldMinIo);
        var minThreads = Math.Max(ConcurrentThreads * 2, 32);
        ThreadPool.SetMinThreads(minThreads, minThreads);

        _cts = new CancellationTokenSource();
        SetupRoutes(_ => Thread.Sleep(HandlerDurationMs));

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
    /// Каждый поток роутит случайные маршруты с медленными обработчиками.
    /// </summary>
    [Benchmark]
    public Task Concurrent_RandomRoutes()
    {
        var token = _cts.Token;
        var tasks = new Task[ConcurrentThreads];
        for (int i = 0; i < ConcurrentThreads; i++)
        {
            var threadId = i;
            tasks[i] = Router.RouteAsync(Routes[_randomGenerators[threadId].Next(0, 100)], token);
        }
        return Task.WhenAll(tasks);
    }

    /// <summary>
    /// Все потоки роутят один маршрут с медленными обработчиками (hot path под нагрузкой).
    /// </summary>
    [Benchmark]
    public Task Concurrent_HotPath()
    {
        var token = _cts.Token;
        var middleRoute = Routes[50];
        var tasks = new Task[ConcurrentThreads];
        for (int i = 0; i < ConcurrentThreads; i++)
        {
            tasks[i] = Router.RouteAsync(middleRoute, token);
        }
        return Task.WhenAll(tasks);
    }
}
