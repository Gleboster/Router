using Factories;
using Interfaces;

namespace Router.Tests.Fixtures;

public sealed class RouterFixture
{
    private readonly RouterFactory _factory = new();

    public IRouter CreateRouter()
    {
        return _factory.CreateRouter();
    }
}
