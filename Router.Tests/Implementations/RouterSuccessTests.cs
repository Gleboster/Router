using Router.Tests.Fixtures;
using Xunit;

namespace Router.Tests.Implementations;

public sealed class RouterSuccessTests
{
    private readonly RouterFixture _fixture = new();

    #region Static Routes

    [Fact]
    public void Register_StaticRoute_InvokesHandler()
    {
        var router = _fixture.CreateRouter();
        var called = false;

        router.RegisterRoute("/foo/bar/", () => called = true);
        router.Route("/foo/bar/");

        Assert.True(called);
    }

    #endregion

    #region Single Parameter Routes

    [Fact]
    public void Register_IntParameter_PassesParsedValue()
    {
        var router = _fixture.CreateRouter();
        var result = 0;

        router.RegisterRoute(
            "/foo/bar/{id:int}/",
            (int id) => result = id
        );

        router.Route("/foo/bar/42/");

        Assert.Equal(42, result);
    }

    #endregion

    #region Multiple Parameters Routes

    [Fact]
    public void Register_MultipleParameters_PassesValuesInOrder()
    {
        var router = _fixture.CreateRouter();
        (int A, int B) result = default;

        router.RegisterRoute(
            "/foo/bar/{a:int}/{b:int}/",
            (int a, int b) => result = (a, b)
        );

        router.Route("/foo/bar/10/20/");

        Assert.Equal((10, 20), result);
    }

    [Fact]
    public void Register_NamedParameters_MapsArgumentsByName()
    {
        var router = _fixture.CreateRouter();
        (int A, int B) result = default;

        router.RegisterRoute(
            "/foo/bar/{a:int}/{b:int}/",
            (int b, int a) => result = (a, b)
        );

        router.Route("/foo/bar/10/20/");

        Assert.Equal((10, 20), result);
    }

    #endregion
}
