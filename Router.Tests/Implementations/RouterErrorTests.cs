using Router.Exceptions;
using Router.Tests.Fixtures;
using Xunit;

namespace Router.Tests.Implementations;

public class RouterErrorTests
{
    private readonly RouterFixture _fixture = new();

    #region Unknown Route Errors

    [Fact]
    public void Invoke_UnknownRoute_Throws()
    {
        var router = _fixture.CreateRouter();

        Assert.Throws<RouteNotFoundException>(
            () => router.Route("/unknown/")
        );
    }

    #endregion

    #region Parameter Type Errors

    [Fact]
    public void Invoke_InvalidIntParameter_Throws()
    {
        var router = _fixture.CreateRouter();

        router.RegisterRoute(
            "/foo/{id:int}/",
            (int id) => { }
        );

        Assert.Throws<RouteNotFoundException>(
            () => router.Route("/foo/not-an-int/")
        );
    }

    #endregion

    #region Registration Errors

    [Fact]
    public void Register_NullTemplate_ThrowsArgumentNullException()
    {
        var router = _fixture.CreateRouter();

        Assert.Throws<ArgumentNullException>(() =>
            router.RegisterRoute(template: null, () => { }));
    }

    [Fact]
    public void Register_NullAction_ThrowsArgumentNullException()
    {
        var router = _fixture.CreateRouter();

        Assert.Throws<ArgumentNullException>(() =>
            router.RegisterRoute("/route", action: null));
    }

    [Fact]
    public void Register_UnknownParameterType_Throws()
    {
        var router = _fixture.CreateRouter();

        Assert.Throws<InvalidTemplateFormatException>(() =>
            router.RegisterRoute(
                "/foo/{id:unknown}/",
                (string id) => { }
            )
        );
    }

    [Fact]
    public void Register_IncompatibleHandlerParameterType_Throws()
    {
        var router = _fixture.CreateRouter();

        Assert.Throws<RouteRegistrationMismatchException>(() =>
            router.RegisterRoute(
                "/foo/{id:int}/",
                (string id) => { }
            )
        );
    }

    #endregion

    #region Duplicate Route Errors

    [Fact]
    public void Register_DuplicateExactTemplate_Throws()
    {
        var router = _fixture.CreateRouter();

        router.RegisterRoute("/foo/{id:int}/", (int id) => { });

        Assert.Throws<DuplicateRouteException>(() =>
            router.RegisterRoute("/foo/{id:int}/", (int id) => { }));
    }

    [Fact]
    public void Register_DuplicateStructuralTemplate_Throws()
    {
        var router = _fixture.CreateRouter();

        router.RegisterRoute("/foo/{id:int}/", (int id) => { });

        Assert.Throws<DuplicateRouteException>(() =>
            router.RegisterRoute("/foo/{value:int}/", (int value) => { }));
    }

    [Fact]
    public void Register_DifferentTemplates_DoesNotThrow()
    {
        var router = _fixture.CreateRouter();

        router.RegisterRoute("/foo/{id:int}/", (int id) => { });
        router.RegisterRoute("/bar/{id:int}/", (int id) => { });
        router.RegisterRoute("/foo/{id:string}/", (string id) => { });

        // Should succeed without exceptions
    }

    #endregion
}
