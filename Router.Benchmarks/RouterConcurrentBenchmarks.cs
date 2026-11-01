using BenchmarkDotNet.Attributes;
using Factories;
using static System.Net.Mime.MediaTypeNames;

namespace Router.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class RouterConcurrentBenchmarks
{
    private Interfaces.IRouter _router = null!;
    private List<string> _routes = null!;
    private CancellationTokenSource _cts = null!;
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

        _router = new RouterFactory().CreateRouter();
        _routes = new List<string>();

        var staticGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var staticDateTime = new DateTime(2024, 1, 1, 12, 0, 0);

        for (int i = 0; i < 100; i++)
        {
            RegisterRouteWithHandler(i, staticGuid, staticDateTime);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cts.Dispose();
        ThreadPool.SetMinThreads(_oldMinWorker, _oldMinIo);
    }

    private void RegisterRouteWithHandler(int index, Guid staticGuid, DateTime staticDateTime)
    {
        var routeType = index % 10;

        switch (routeType)
        {
            case 0:
            _router.RegisterRoute<int>($"/api/users{index}/{{id:int}}/", (id) => { });
            _routes.Add($"/api/users{index}/{1000 + index}/");
            break;

            case 1:
            _router.RegisterRoute<int>($"/api/products{index}/{{id:int}}/details/", (id) => { });
            _routes.Add($"/api/products{index}/{1000 + index}/details/");
            break;

            case 2:
            _router.RegisterRoute<Guid>($"/api/orders{index}/{{orderId:guid}}/", (orderId) => { });
            _routes.Add($"/api/orders{index}/{staticGuid}/");
            break;

            case 3:
            _router.RegisterRoute<string>($"/api/categories{index}/{{name:string}}/", (name) => { });
            _routes.Add($"/api/categories{index}/category{index}/");
            break;

            case 4:
            _router.RegisterRoute<int, string>($"/api/items{index}/{{id:int}}/price/{{currency:string}}/", (id, currency) => { });
            _routes.Add($"/api/items{index}/{1000 + index}/price/USD/");
            break;

            case 5:
            _router.RegisterRoute($"/api/static{index}/route{index}/", () => { });
            _routes.Add($"/api/static{index}/route{index}/");
            break;

            case 6:
            _router.RegisterRoute($"/api/v{index}/resource{index}/", () => { });
            _routes.Add($"/api/v{index}/resource{index}/");
            break;

            case 7:
            _router.RegisterRoute<DateTime>($"/api/data{index}/{{timestamp:datetime}}/", (timestamp) => { });
            _routes.Add($"/api/data{index}/{staticDateTime:yyyy-MM-ddTHH:mm:ss}/");
            break;

            case 8:
            _router.RegisterRoute<float>($"/api/values{index}/{{value:float}}/", (value) => { });
            _routes.Add($"/api/values{index}/{(123.45f + index).ToString(System.Globalization.CultureInfo.InvariantCulture)}/");
            break;

            case 9:
            _router.RegisterRoute<int>($"/api/test{index}/{{id:int}}/", (id) => { });
            _routes.Add($"/api/test{index}/{1000 + index}/");
                break;
        }
    }

    private Task RunConcurrentAsync(Func<int, string> routeSelector)
    {
        var token = _cts.Token;
        var tasks = new Task[ConcurrentThreads];
        for (int i = 0; i < ConcurrentThreads; i++)
        {
            tasks[i] = _router.RouteAsync(routeSelector(i), token);
        }
        return Task.WhenAll(tasks);
    }

    /// <summary>
    /// Random на каждого отправителя
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    public Task Concurrent_RandomRoutes_100RequestsPerThread()
    {
        var rnd = new Random[ConcurrentThreads];
        for (int i = 0; i < ConcurrentThreads; i++)
            rnd[i] = new Random(42 + i);

        return RunConcurrentAsync(i => _routes[rnd[i].Next(0, 100)]);
    }

    /// <summary>
    /// Все отправители дёргают один и тот же (средний) маршрут.
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    public Task Concurrent_HotPath_SameRoute()
    {
        var middleRoute = _routes[50];
        return RunConcurrentAsync(_ => middleRoute);
    }

    /// <summary>
    /// Каждый отправитель идёт по своему диапазону маршрутов:
    /// i-й отправитель берёт routeIndex = i % 100.
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    public Task Concurrent_MixedPattern_DistributedLoad()
    {
        return RunConcurrentAsync(i => _routes[i % 100]);
    }

    /// <summary>
    /// Все отправители дёргают последний маршрут — худший случай обхода trie.
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    public Task Concurrent_WorstCase_LastRoute()
    {
        var lastRoute = _routes[99];
        return RunConcurrentAsync(_ => lastRoute);
    }
}