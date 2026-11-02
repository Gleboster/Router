using Router.Factories;
using Router.Interfaces;

namespace Router.Tests.Fixtures;

public class RouterFixture
{
    private readonly RouterFactory _factory = new();

    public IRouter CreateRouter()
    {
        return _factory.CreateRouter();
    }
}
