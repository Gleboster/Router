using System.Threading;
using System.Threading.Tasks;
using Router.Tests.Fixtures;
using Xunit;

namespace Router.Tests.Implementations;

public class RouterSuccessTests
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

    #region Async Routes

    [Fact]
    public async Task RouteAsync_ValidRoute_InvokesHandler()
    {
        var router = _fixture.CreateRouter();
        var invoked = false;
        router.RegisterRoute("/test", () => invoked = true);

        await router.RouteAsync("/test", CancellationToken.None);

        Assert.True(invoked);
    }

    [Fact]
    public async Task RouteAsync_CancellationRequested_BeforeRun_ThrowsOperationCanceledException()
    {
        var router = _fixture.CreateRouter();
        router.RegisterRoute("/test", () => Thread.Sleep(5000));

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            router.RouteAsync("/test", cts.Token));
    }


    [Fact]
    public async Task RouteAsync_CancellationRequested_DuringRun_DoesNotCancelHandler()
    {
        var router = _fixture.CreateRouter();
        var cts = new CancellationTokenSource();
        var handlerCompleted = false;

        router.RegisterRoute("/test", () =>
        {
            Thread.Sleep(500); 
            handlerCompleted = true;
        });

        cts.CancelAfter(50);

        await router.RouteAsync("/test", cts.Token);


        Assert.True(handlerCompleted, 
            "Handler should complete despite token cancellation during execution");
    }

    #endregion
}
