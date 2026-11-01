using BenchmarkDotNet.Attributes;

namespace Router.Benchmarks;

/// <summary>
/// Benchmark 1: Один поток, один запрос - минимальная накладная на маршрутизатор.
/// </summary>
[MemoryDiagnoser]
public class SingleThreadBenchmarks : BenchmarkBase
{
    private readonly Random _random = new(42);

    [GlobalSetup]
    public void Setup()
    {
        SetupRoutes(_ => { });
    }

    /// <summary>
    /// Роутинг к первому маршруту (лучший случай - мгновенное совпадение).
    /// </summary>
    [Benchmark]
    public void Route_FirstMatch()
    {
        Router.Route(Routes[0]);
    }

    /// <summary>
    /// Роутинг к среднему маршруту (средний случай).
    /// </summary>
    [Benchmark]
    public void Route_MiddleMatch()
    {
        Router.Route(Routes[50]);
    }

    /// <summary>
    /// Роутинг к последнему маршруту (худший случай - полный обход Trie).
    /// </summary>
    [Benchmark]
    public void Route_LastMatch()
    {
        Router.Route(Routes[99]);
    }

    /// <summary>
    /// Роутинг к случайному маршруту (реалистичный случай).
    /// </summary>
    [Benchmark]
    public void Route_RandomMatch()
    {
        var randomIndex = _random.Next(0, 100);
        Router.Route(Routes[randomIndex]);
    }
}
