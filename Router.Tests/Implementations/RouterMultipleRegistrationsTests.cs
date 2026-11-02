using Router.Factories;
using Xunit;

namespace Router.Tests.Implementations;

public class RouterMultipleRegistrationsTests
{
    #region Registration and Resolution Tests

    [Fact]
    public void RegisterAndResolve_100UniqueRoutes_AllShouldSucceed()
    {
        // Arrange
        var router = new RouterFactory().CreateRouter();
        var routes = new List<string>();
        var staticGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var staticDateTime = new DateTime(2024, 1, 1, 12, 0, 0);

        // Act - Register 100 routes
        for (int i = 0; i < 100; i++)
        {
            RegisterRouteWithHandler(router, routes, i, staticGuid, staticDateTime);
        }

        // Assert - All routes should be registered
        Assert.Equal(100, routes.Count);

        // Act & Assert - Try to resolve each route
        var failedRoutes = new List<(int index, string route, Exception error)>();

        for (int i = 0; i < routes.Count; i++)
        {
            try
            {
                router.Route(routes[i]);
            }
            catch (Exception ex)
            {
                failedRoutes.Add((i, routes[i], ex));
            }
        }

        // Print failed routes for diagnostics
        if (failedRoutes.Any())
        {
            var errorReport = string.Join("\n",
                failedRoutes.Select(f =>
                    $"Route #{f.index}: {f.route}\n  Error: {f.error.GetType().Name}: {f.error.Message}"));

            Assert.Fail($"Failed to resolve {failedRoutes.Count} out of {routes.Count} routes:\n{errorReport}");
        }
    }

    [Fact]
    public void RegisterAndResolve_CheckEachRouteType_ShouldSucceed()
    {
        // Arrange
        var router = new RouterFactory().CreateRouter();
        var staticGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var staticDateTime = new DateTime(2024, 1, 1, 12, 0, 0);

        // Test each route type (0-9)
        for (int routeType = 0; routeType < 10; routeType++)
        {
            var index = routeType;
            var routes = new List<string>();

            // Act - Register one route of this type
            RegisterRouteWithHandler(router, routes, index, staticGuid, staticDateTime);

            // Assert - Should be able to resolve it
            var exception = Record.Exception(() => router.Route(routes[0]));

            Assert.Null(exception);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(99)]
    public void RegisterAndResolve_SpecificIndices_ShouldSucceed(int index)
    {
        // Arrange
        var router = new RouterFactory().CreateRouter();
        var routes = new List<string>();
        var staticGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var staticDateTime = new DateTime(2024, 1, 1, 12, 0, 0);

        // Act - Register the specific route
        RegisterRouteWithHandler(router, routes, index, staticGuid, staticDateTime);

        // Assert - Should be able to resolve it
        var exception = Record.Exception(() => router.Route(routes[0]));

        Assert.Null(exception);
    }

    #endregion

    #region Helper Methods

    private void RegisterRouteWithHandler(Interfaces.IRouter router, List<string> routes, int index, Guid staticGuid, DateTime staticDateTime)
    {
        var routeType = index % 10;

        switch (routeType)
        {
            case 0:
                router.RegisterRoute<int>($"/api/users{index}/{{id:int}}/", (id) => { });
                routes.Add($"/api/users{index}/{1000 + index}/");
                break;

            case 1:
                router.RegisterRoute<int>($"/api/products{index}/{{id:int}}/details/", (id) => { });
                routes.Add($"/api/products{index}/{1000 + index}/details/");
                break;

            case 2:
                router.RegisterRoute<Guid>($"/api/orders{index}/{{orderId:guid}}/", (orderId) => { });
                routes.Add($"/api/orders{index}/{staticGuid}/");
                break;

            case 3:
                router.RegisterRoute<string>($"/api/categories{index}/{{name:string}}/", (name) => { });
                routes.Add($"/api/categories{index}/category{index}/");
                break;

            case 4:
                router.RegisterRoute<int, string>($"/api/items{index}/{{id:int}}/price/{{currency:string}}/", (id, currency) => { });
                routes.Add($"/api/items{index}/{1000 + index}/price/USD/");
                break;

            case 5:
                router.RegisterRoute($"/api/static{index}/route{index}/", () => { });
                routes.Add($"/api/static{index}/route{index}/");
                break;

            case 6:
                router.RegisterRoute($"/api/v{index}/resource{index}/", () => { });
                routes.Add($"/api/v{index}/resource{index}/");
                break;

            case 7:
                router.RegisterRoute<DateTime>($"/api/data{index}/{{timestamp:datetime}}/", (timestamp) => { });
                routes.Add($"/api/data{index}/{staticDateTime:yyyy-MM-ddTHH:mm:ss}/");
                break;

            case 8:
                router.RegisterRoute<float>($"/api/values{index}/{{value:float}}/", (value) => { });
                routes.Add($"/api/values{index}/{(123.45f + index).ToString(System.Globalization.CultureInfo.InvariantCulture)}/");
                break;

            case 9:
                router.RegisterRoute<int>($"/api/test{index}/{{id:int}}/", (id) => { });
                routes.Add($"/api/test{index}/{1000 + index}/");
                break;
        }
    }

    #endregion
}
