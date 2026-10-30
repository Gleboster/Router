using BenchmarkDotNet.Attributes;
using Factories;

namespace Router.Benchmarks;

[MemoryDiagnoser]
public class RouterBenchmarks
{
    private Interfaces.IRouter _router = null!;
    private readonly Random _random = new(42);
    private List<string> _routes = null!;

    [GlobalSetup]
    public void Setup()
    {
        _router = new RouterFactory().CreateRouter();
        _routes = new List<string>();

        var staticGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var staticDateTime = new DateTime(2024, 1, 1, 12, 0, 0);

        for (int i = 0; i < 100; i++)
        {
            RegisterRouteWithHandler(i, staticGuid, staticDateTime);
        }
    }

    private void RegisterRouteWithHandler(int index, Guid staticGuid, DateTime staticDateTime)
    {
        var routeType = index % 10;

        switch (routeType)
        {
            case 0:
                _router.RegisterRoute<int>($"/api/users/{{id:int}}/", (id) => { });
                _routes.Add($"/api/users/{1000 + index}/");
                break;

            case 1:
                _router.RegisterRoute<int>($"/api/products/{{id:int}}/details/", (id) => { });
                _routes.Add($"/api/products/{1000 + index}/details/");
                break;

            case 2:
                _router.RegisterRoute<Guid>($"/api/orders/{{orderId:guid}}/", (orderId) => { });
                _routes.Add($"/api/orders/{staticGuid}/");
                break;

            case 3:
                _router.RegisterRoute<string>($"/api/categories/{{name:string}}/", (name) => { });
                _routes.Add($"/api/categories/category{index}/");
                break;

            case 4:
                _router.RegisterRoute<int, string>($"/api/items/{{id:int}}/price/{{currency:string}}/", (id, currency) => { });
                _routes.Add($"/api/items/{1000 + index}/price/USD/");
                break;

            case 5:
                _router.RegisterRoute($"/api/static/route{index}/", () => { });
                _routes.Add($"/api/static/route{index}/");
                break;

            case 6:
                _router.RegisterRoute($"/api/v{index}/resource/", () => { });
                _routes.Add($"/api/v{index}/resource/");
                break;

            case 7:
                _router.RegisterRoute<DateTime>($"/api/data/{{timestamp:datetime}}/", (timestamp) => { });
                _routes.Add($"/api/data/{staticDateTime:yyyy-MM-ddTHH:mm:ss}/");
                break;

            case 8:
                _router.RegisterRoute<float>($"/api/values/{{value:float}}/", (value) => { });
                _routes.Add($"/api/values/{123.45 + index}/");
                break;

            case 9:
                _router.RegisterRoute<int>($"/api/test{index}/{{id:int}}/", (id) => { });
                _routes.Add($"/api/test{index}/{1000 + index}/");
                break;
        }
    }

    [Benchmark]
    public void RouteFirstMatch()
    {
        _router.Route(_routes[0]);
    }

    [Benchmark]
    public void RouteMiddleMatch()
    {
        _router.Route(_routes[50]);
    }

    [Benchmark]
    public void RouteLastMatch()
    {
        _router.Route(_routes[99]);
    }

    [Benchmark]
    public void RouteRandomMatch()
    {
        var randomIndex = _random.Next(0, 100);
        _router.Route(_routes[randomIndex]);
    }
}
