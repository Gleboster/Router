namespace Router.Benchmarks;

/// <summary>
/// Базовый класс для всех бенчмарков роутера с общей логикой настройки.
/// </summary>
public abstract class BenchmarkBase
{
    protected Interfaces.IRouter Router = null!;
    protected List<string> Routes = null!;

    protected void SetupRoutes(Action<int> handlerAction)
    {
        Router = new Factories.RouterFactory().CreateRouter();
        Routes = new List<string>();

        var staticGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var staticDateTime = new DateTime(2024, 1, 1, 12, 0, 0);

        for (int i = 0; i < 100; i++)
        {
            RegisterRouteWithHandler(i, staticGuid, staticDateTime, handlerAction);
        }
    }

    private void RegisterRouteWithHandler(
        int index, 
        Guid staticGuid, 
        DateTime staticDateTime, 
        Action<int> handlerAction)
    {
        var routeType = index % 10;

        switch (routeType)
        {
            case 0:
                Router.RegisterRoute<int>($"/api/users{index}/{{id:int}}/", id => handlerAction(id));
                Routes.Add($"/api/users{index}/{1000 + index}/");
                break;

            case 1:
                Router.RegisterRoute<int>($"/api/products{index}/{{id:int}}/details/", id => handlerAction(id));
                Routes.Add($"/api/products{index}/{1000 + index}/details/");
                break;

            case 2:
                Router.RegisterRoute<Guid>($"/api/orders{index}/{{orderId:guid}}/", orderId => handlerAction(index));
                Routes.Add($"/api/orders{index}/{staticGuid}/");
                break;

            case 3:
                Router.RegisterRoute<string>($"/api/categories{index}/{{name:string}}/", name => handlerAction(index));
                Routes.Add($"/api/categories{index}/category{index}/");
                break;

            case 4:
                Router.RegisterRoute<int, string>($"/api/items{index}/{{id:int}}/price/{{currency:string}}/", (id, currency) => handlerAction(id));
                Routes.Add($"/api/items{index}/{1000 + index}/price/USD/");
                break;

            case 5:
                Router.RegisterRoute($"/api/static{index}/route{index}/", () => handlerAction(index));
                Routes.Add($"/api/static{index}/route{index}/");
                break;

            case 6:
                Router.RegisterRoute($"/api/v{index}/resource{index}/", () => handlerAction(index));
                Routes.Add($"/api/v{index}/resource{index}/");
                break;

            case 7:
                Router.RegisterRoute<DateTime>($"/api/data{index}/{{timestamp:datetime}}/", timestamp => handlerAction(index));
                Routes.Add($"/api/data{index}/{staticDateTime:yyyy-MM-ddTHH:mm:ss}/");
                break;

            case 8:
                Router.RegisterRoute<float>($"/api/values{index}/{{value:float}}/", value => handlerAction(index));
                Routes.Add($"/api/values{index}/{(123.45f + index).ToString(System.Globalization.CultureInfo.InvariantCulture)}/");
                break;

            case 9:
                Router.RegisterRoute<int>($"/api/test{index}/{{id:int}}/", id => handlerAction(id));
                Routes.Add($"/api/test{index}/{1000 + index}/");
                break;
        }
    }
}
