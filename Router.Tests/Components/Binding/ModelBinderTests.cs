using Router.Components.Binding;
using Router.Components.Compilers;
using Router.Models.Routes;
using Router.Models.Segments;
using Router.Models.Templates;
using Xunit;

namespace Router.Tests.Components.Binding;

public sealed class ModelBinderTests
{
    private readonly ModelBinder _binder = new();
    private readonly TemplateParser _parser = new();

    #region Single Parameter Binding

    [Fact]
    public void ModelBind_IntParameter_ReturnsCorrectValue()
    {
        var template = CreateTemplate("/users/{id:int}");
        var route = CreateRoute("users", "42");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(42, result.GetValueByName<int>("id"));
    }

    [Fact]
    public void ModelBind_StringParameter_ReturnsCorrectValue()
    {
        var template = CreateTemplate("/users/{name:string}");
        var route = CreateRoute("users", "john");

        var result = _binder.ModelBind(template, route);

        Assert.Equal("john", result.GetValueByName<string>("name"));
    }

    [Fact]
    public void ModelBind_GuidParameter_ReturnsCorrectValue()
    {
        var template = CreateTemplate("/items/{id:guid}");
        var expectedGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var route = CreateRoute("items", "550e8400-e29b-41d4-a716-446655440000");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(expectedGuid, result.GetValueByName<Guid>("id"));
    }

    [Fact]
    public void ModelBind_FloatParameter_ReturnsCorrectValue()
    {
        var template = CreateTemplate("/price/{value:float}");
        var route = CreateRoute("price", "19.99");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(19.99f, result.GetValueByName<float>("value"), precision: 2);
    }

    [Fact]
    public void ModelBind_DateTimeParameter_ReturnsCorrectValue()
    {
        var template = CreateTemplate("/date/{when:datetime}");
        var expectedDate = DateTime.Parse("2026-10-30T12:00:00Z").ToUniversalTime();
        var route = CreateRoute("date", "2026-10-30T12:00:00Z");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(expectedDate, result.GetValueByName<DateTime>("when"));
    }

    #endregion

    #region Multiple Parameters Binding

    [Fact]
    public void ModelBind_MultipleIntParameters_ReturnsAllValues()
    {
        var template = CreateTemplate("/foo/{a:int}/{b:int}");
        var route = CreateRoute("foo", "10", "20");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(10, result.GetValueByName<int>("a"));
        Assert.Equal(20, result.GetValueByName<int>("b"));
    }

    [Fact]
    public void ModelBind_MixedTypeParameters_ReturnsCorrectTypes()
    {
        var template = CreateTemplate("/user/{id:int}/post/{guid:guid}");
        var expectedGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var route = CreateRoute("user", "123", "post", "550e8400-e29b-41d4-a716-446655440000");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(123, result.GetValueByName<int>("id"));
        Assert.Equal(expectedGuid, result.GetValueByName<Guid>("guid"));
    }

    [Fact]
    public void ModelBind_ThreeParameters_ReturnsAllValues()
    {
        var template = CreateTemplate("/a/{x:int}/b/{y:string}/c/{z:float}");
        var route = CreateRoute("a", "1", "b", "test", "c", "3.14");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(1, result.GetValueByName<int>("x"));
        Assert.Equal("test", result.GetValueByName<string>("y"));
        Assert.Equal(3.14f, result.GetValueByName<float>("z"), precision: 2);
    }

    #endregion

    #region Static and Dynamic Segments

    [Fact]
    public void ModelBind_StaticPrefix_IgnoresStaticSegments()
    {
        var template = CreateTemplate("/api/users/{id:int}");
        var route = CreateRoute("api", "users", "42");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(42, result.GetValueByName<int>("id"));
    }

    [Fact]
    public void ModelBind_StaticBetweenDynamic_MapsCorrectly()
    {
        var template = CreateTemplate("/{id:int}/profile/{name:string}");
        var route = CreateRoute("123", "profile", "john");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(123, result.GetValueByName<int>("id"));
        Assert.Equal("john", result.GetValueByName<string>("name"));
    }

    [Fact]
    public void ModelBind_MultipleStaticSegments_PositionsMatch()
    {
        var template = CreateTemplate("/a/b/{x:int}/c/d/{y:int}");
        var route = CreateRoute("a", "b", "10", "c", "d", "20");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(10, result.GetValueByName<int>("x"));
        Assert.Equal(20, result.GetValueByName<int>("y"));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ModelBind_NoParameters_ReturnsEmptyResult()
    {
        var template = CreateTemplate("/foo/bar/baz");
        var route = CreateRoute("foo", "bar", "baz");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(0, template.CountVariables);
    }

    [Fact]
    public void ModelBind_EmptyStringValue_ReturnsEmptyString()
    {
        var template = CreateTemplate("/{name:string}/profile");
        var route = CreateRoute("", "profile");

        var result = _binder.ModelBind(template, route);

        Assert.Equal("", result.GetValueByName<string>("name"));
    }

    [Fact]
    public void ModelBind_SpecialCharactersInString_HandlesCorrectly()
    {
        var template = CreateTemplate("/{text:string}");
        var route = CreateRoute("hello-world_123");

        var result = _binder.ModelBind(template, route);

        Assert.Equal("hello-world_123", result.GetValueByName<string>("text"));
    }

    #endregion

    #region Type Conversion Edge Cases

    [Fact]
    public void ModelBind_IntMinValue_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:int}");
        var route = CreateRoute("-2147483648");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(int.MinValue, result.GetValueByName<int>("value"));
    }

    [Fact]
    public void ModelBind_IntMaxValue_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:int}");
        var route = CreateRoute("2147483647");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(int.MaxValue, result.GetValueByName<int>("value"));
    }

    [Fact]
    public void ModelBind_IntZero_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:int}");
        var route = CreateRoute("0");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(0, result.GetValueByName<int>("value"));
    }

    [Fact]
    public void ModelBind_NegativeInt_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:int}");
        var route = CreateRoute("-123");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(-123, result.GetValueByName<int>("value"));
    }

    [Fact]
    public void ModelBind_FloatZero_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:float}");
        var route = CreateRoute("0.0");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(0.0f, result.GetValueByName<float>("value"));
    }

    [Fact]
    public void ModelBind_NegativeFloat_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:float}");
        var route = CreateRoute("-3.14");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(-3.14f, result.GetValueByName<float>("value"), precision: 2);
    }

    [Fact]
    public void ModelBind_VerySmallFloat_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:float}");
        var route = CreateRoute("0.00001");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(0.00001f, result.GetValueByName<float>("value"), precision: 5);
    }

    [Fact]
    public void ModelBind_GuidWithHyphens_ParsesCorrectly()
    {
        var template = CreateTemplate("/{id:guid}");
        var expectedGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var route = CreateRoute("550e8400-e29b-41d4-a716-446655440000");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(expectedGuid, result.GetValueByName<Guid>("id"));
    }

    [Fact]
    public void ModelBind_DateTimeISO8601_ParsesCorrectly()
    {
        var template = CreateTemplate("/{when:datetime}");
        var expectedDate = DateTime.Parse("2026-10-30T10:30:00Z").ToUniversalTime();
        var route = CreateRoute("2026-10-30T10:30:00Z");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(expectedDate, result.GetValueByName<DateTime>("when"));
    }

    [Fact]
    public void ModelBind_DateTimeWithoutTime_ParsesCorrectly()
    {
        var template = CreateTemplate("/{when:datetime}");
        var route = CreateRoute("2026-10-30");

        var result = _binder.ModelBind(template, route);

        var parsedDate = result.GetValueByName<DateTime>("when");
        Assert.Equal(2026, parsedDate.Year);
        Assert.Equal(10, parsedDate.Month);
        Assert.Equal(30, parsedDate.Day);
    }

    #endregion

    #region Error Handling - Invalid Type Conversions

    [Fact]
    public void ModelBind_InvalidIntValue_ThrowsException()
    {
        var template = CreateTemplate("/{id:int}");
        var route = CreateRoute("abc");

        Assert.ThrowsAny<Exception>(() => _binder.ModelBind(template, route));
    }

    [Fact]
    public void ModelBind_InvalidFloatValue_ThrowsException()
    {
        var template = CreateTemplate("/{value:float}");
        var route = CreateRoute("not-a-number");

        Assert.ThrowsAny<Exception>(() => _binder.ModelBind(template, route));
    }

    [Fact]
    public void ModelBind_InvalidGuidValue_ThrowsException()
    {
        var template = CreateTemplate("/{id:guid}");
        var route = CreateRoute("not-a-guid");

        Assert.ThrowsAny<Exception>(() => _binder.ModelBind(template, route));
    }

    [Fact]
    public void ModelBind_InvalidDateTimeValue_ThrowsException()
    {
        var template = CreateTemplate("/{when:datetime}");
        var route = CreateRoute("invalid-date");

        Assert.ThrowsAny<Exception>(() => _binder.ModelBind(template, route));
    }

    #endregion

    #region Error Handling - Segment Count Mismatch

    [Fact]
    public void ModelBind_MoreDynamicSegmentsThanRouteValues_ThrowsException()
    {
        var template = CreateTemplate("/{a:int}/{b:int}/{c:int}");
        var route = CreateRoute("10", "20");

        Assert.ThrowsAny<Exception>(() => _binder.ModelBind(template, route));
    }

    #endregion

    #region ModelBindResult Behavior

    [Fact]
    public void GetValueByName_WithCorrectType_ReturnsValue()
    {
        var template = CreateTemplate("/{id:int}");
        var route = CreateRoute("42");

        var result = _binder.ModelBind(template, route);

        Assert.Equal(42, result.GetValueByName<int>("id"));
    }

    [Fact]
    public void GetValueByName_WithWrongType_ThrowsInvalidOperationException()
    {
        var template = CreateTemplate("/{id:int}");
        var route = CreateRoute("42");

        var result = _binder.ModelBind(template, route);

        Assert.Throws<InvalidOperationException>(() => result.GetValueByName<string>("id"));
    }

    [Fact]
    public void GetValueByName_WithMissingName_ThrowsKeyNotFoundException()
    {
        var template = CreateTemplate("/{id:int}");
        var route = CreateRoute("42");

        var result = _binder.ModelBind(template, route);

        Assert.Throws<KeyNotFoundException>(() => result.GetValueByName<int>("nonexistent"));
    }

    [Fact]
    public void GetValueByName_CaseSensitive_ThrowsOnWrongCase()
    {
        var template = CreateTemplate("/{userId:int}");
        var route = CreateRoute("42");

        var result = _binder.ModelBind(template, route);

        Assert.Throws<KeyNotFoundException>(() => result.GetValueByName<int>("userid"));
    }

    #endregion

    #region Helper Methods

    private Template CreateTemplate(string pattern)
    {
        return _parser.Parse(pattern);
    }

    private Route CreateRoute(params string[] segmentValues)
    {
        var segments = segmentValues.Select(v => new RouteSegment(v));
        return new Route(segments);
    }

    #endregion
}
