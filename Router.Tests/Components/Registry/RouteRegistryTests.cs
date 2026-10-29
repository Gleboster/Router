using Router.Components.Compilers;
using Router.Components.Registry;
using Router.Models.Registrations;
using Router.Models.Routes;
using Router.Models.Segments;
using Xunit;

namespace Router.Tests.Components.Registry;

public sealed class RouteRegistryTests
{
    private readonly RouteRegistry _registry;
    private readonly TemplateParser _templateParser;
    private readonly HandlerMetadataParser _handlerParser;

    public RouteRegistryTests()
    {
        _registry = new RouteRegistry();
        _templateParser = new TemplateParser();
        _handlerParser = new HandlerMetadataParser();
    }

    #region Static Routes

    [Fact]
    public void Save_StaticRoute_CanBeResolved()
    {
        // Arrange
        var registration = CreateRegistration("/foo/bar", () => { });
        var route = CreateRoute("/foo/bar");

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    [Fact]
    public void Save_MultipleDifferentStaticRoutes_EachCanBeResolved()
    {
        // Arrange
        var registration1 = CreateRegistration("/foo/bar", () => { });
        var registration2 = CreateRegistration("/foo/baz", () => { });
        var registration3 = CreateRegistration("/api/users", () => { });

        // Act
        _registry.Save(registration1);
        _registry.Save(registration2);
        _registry.Save(registration3);

        // Assert
        var result1 = _registry.ResolveRoute(CreateRoute("/foo/bar"));
        var result2 = _registry.ResolveRoute(CreateRoute("/foo/baz"));
        var result3 = _registry.ResolveRoute(CreateRoute("/api/users"));

        Assert.NotNull(result1);
        Assert.Same(registration1, result1);

        Assert.NotNull(result2);
        Assert.Same(registration2, result2);

        Assert.NotNull(result3);
        Assert.Same(registration3, result3);
    }

    [Fact]
    public void Save_SimilarStaticRoutes_DoNotConflict()
    {
        // Arrange
        var registration1 = CreateRegistration("/users/list", () => { });
        var registration2 = CreateRegistration("/users/details", () => { });

        // Act
        _registry.Save(registration1);
        _registry.Save(registration2);

        // Assert
        var result1 = _registry.ResolveRoute(CreateRoute("/users/list"));
        var result2 = _registry.ResolveRoute(CreateRoute("/users/details"));

        Assert.NotNull(result1);
        Assert.Same(registration1, result1);

        Assert.NotNull(result2);
        Assert.Same(registration2, result2);
    }

    #endregion

    #region Dynamic Routes

    [Fact]
    public void Save_RouteWithIntParameter_CanBeResolved()
    {
        // Arrange
        var registration = CreateRegistration("/users/{id:int}", (int id) => { });
        var route = CreateRoute("/users/42");

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    [Theory]
    [InlineData("/users/{id:string}", "/users/john")]
    [InlineData("/posts/{id:guid}", "/posts/550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("/prices/{value:float}", "/prices/19.99")]
    [InlineData("/events/{date:datetime}", "/events/2024-01-15")]
    public void Save_RouteWithDifferentParameterTypes_CanBeResolved(
        string templateStr,
        string routeStr)
    {
        // Arrange
        var template = _templateParser.Parse(templateStr);
        var handler = _handlerParser.Parse((string value) => { });
        var registration = new RouteRegistration(template, handler);
        var route = CreateRoute(routeStr);

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    #endregion

    #region Combined Routes

    [Fact]
    public void Save_RouteWithMultipleParameters_CanBeResolved()
    {
        // Arrange
        var registration = CreateRegistration(
            "/api/users/{userId:guid}/posts/{postId:int}",
            (Guid userId, int postId) => { });
        var route = CreateRoute("/api/users/550e8400-e29b-41d4-a716-446655440000/posts/123");

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    [Fact]
    public void Save_RouteWithStaticSegmentsBetweenParameters_MatchesCorrectly()
    {
        // Arrange
        var registration = CreateRegistration(
            "/api/{version:int}/users/{id:int}/profile",
            (int version, int id) => { });
        var route = CreateRoute("/api/1/users/42/profile");

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    #endregion

    #region Registration Priority

    [Fact]
    public void Save_DynamicRouteRegisteredFirst_TakesPrecedenceOverStaticRoute()
    {
        // Arrange
        var dynamicRegistration = CreateRegistration("/users/{id:string}", (string id) => { });
        var staticRegistration = CreateRegistration("/users/admin", () => { });

        // Act
        _registry.Save(dynamicRegistration); // Зарегистрирован первым
        _registry.Save(staticRegistration);

        var route = CreateRoute("/users/admin");
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(dynamicRegistration, result);
    }

    [Fact]
    public void Save_StaticRouteRegisteredFirst_TakesPrecedenceOverDynamicRoute()
    {
        // Arrange
        var staticRegistration = CreateRegistration("/users/admin", () => { });
        var dynamicRegistration = CreateRegistration("/users/{id:string}", (string id) => { });

        // Act
        _registry.Save(staticRegistration); // Зарегистрирован первым
        _registry.Save(dynamicRegistration);

        var route = CreateRoute("/users/admin");
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(staticRegistration, result);
    }

    [Fact]
    public void Save_DynamicRouteRegisteredFirst_DoesNotBlockDeeperStaticRoute()
    {
        // Arrange
        var dynamicRegistration = CreateRegistration("/users/{id:string}", (string id) => { });
        var staticRegistration = CreateRegistration("/users/admin/settings", () => { });

        // Act
        _registry.Save(dynamicRegistration);
        _registry.Save(staticRegistration);

        var route = CreateRoute("/users/admin/settings");
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(staticRegistration, result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Save_RootRoute_CanBeResolved()
    {
        // Arrange
        var registration = CreateRegistration("/", () => { });
        var route = CreateRoute("/");

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    [Fact]
    public void Save_SingleSegmentRoute_CanBeResolved()
    {
        // Arrange
        var registration = CreateRegistration("/users", () => { });
        var route = CreateRoute("/users");

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    [Fact]
    public void Save_RouteWithOnlyParameter_CanBeResolved()
    {
        // Arrange
        var registration = CreateRegistration("/{id:int}", (int id) => { });
        var route = CreateRoute("/123");

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    #endregion

    #region Multiple Parameters

    [Fact]
    public void Save_RouteWithThreeConsecutiveIntParameters_CanBeResolved()
    {
        // Arrange
        var registration = CreateRegistration(
            "/api/{v1:int}/{v2:int}/{v3:int}",
            (int v1, int v2, int v3) => { });
        var route = CreateRoute("/api/10/20/30");

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    [Fact]
    public void Save_RouteWithMultipleSameTypeParameters_CanBeResolved()
    {
        // Arrange
        var registration = CreateRegistration(
            "/users/{userId:int}/posts/{postId:int}/comments/{commentId:int}",
            (int userId, int postId, int commentId) => { });
        var route = CreateRoute("/users/1/posts/2/comments/3");

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    #endregion

    #region Trailing Slash

    [Fact]
    public void Save_RouteWithTrailingSlash_MatchesRouteWithoutTrailingSlash()
    {
        // Arrange
        var registration = CreateRegistration("/foo/bar/", () => { });
        var route = CreateRoute("/foo/bar"); // Без слэша

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    [Fact]
    public void Save_RouteWithoutTrailingSlash_MatchesRouteWithTrailingSlash()
    {
        // Arrange
        var registration = CreateRegistration("/foo/bar", () => { });
        var route = CreateRoute("/foo/bar/"); // Со слэшем

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    [Fact]
    public void Save_ParameterizedRouteWithTrailingSlash_MatchesWithoutTrailingSlash()
    {
        // Arrange
        var registration = CreateRegistration("/users/{id:int}/", (int id) => { });
        var route = CreateRoute("/users/42"); // Без слэша

        // Act
        _registry.Save(registration);
        var result = _registry.ResolveRoute(route);

        // Assert
        Assert.NotNull(result);
        Assert.Same(registration, result);
    }

    #endregion

    #region Helper Methods

    private RouteRegistration CreateRegistration(string templateStr, Delegate handler)
    {
        var template = _templateParser.Parse(templateStr);
        var handlerMetadata = _handlerParser.Parse(handler);
        return new RouteRegistration(template, handlerMetadata);
    }

    private Route CreateRoute(string path)
    {
        // TODO: Когда RouteParser будет реализован, использовать его
        // Сейчас создаём Route напрямую из сегментов
        var segments = path
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => new RouteSegment(s))
            .ToList();

        return new Route(segments);
    }

    #endregion
}
