using Router.Components.Binding;
using Router.Components.Compilers;
using Router.Exceptions;
using Router.Models.Routes;
using Router.Models.Segments;
using Router.Models.Templates;
using System.Reflection.Metadata;
using Xunit;

namespace Router.Tests.Components.Binding;

public class ModelBinderTests
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

        var modelBindObject = result.GetValueByName("id");
        Assert.Equal("id", modelBindObject.Name);
        Assert.Equal(42, modelBindObject.Value);
        Assert.Equal(typeof(int), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_StringParameter_ReturnsCorrectValue()
    {
        var template = CreateTemplate("/users/{name:string}");
        var route = CreateRoute("users", "john");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("name");
        Assert.Equal("name", modelBindObject.Name);
        Assert.Equal("john", modelBindObject.Value);
        Assert.Equal(typeof(string), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_GuidParameter_ReturnsCorrectValue()
    {
        var template = CreateTemplate("/items/{id:guid}");
        var expectedGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var route = CreateRoute("items", "550e8400-e29b-41d4-a716-446655440000");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("id");
        Assert.Equal("id", modelBindObject.Name);
        Assert.Equal(expectedGuid, modelBindObject.Value);
        Assert.Equal(typeof(Guid), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_FloatParameter_ReturnsCorrectValue()
    {
        var template = CreateTemplate("/price/{value:float}");
        var route = CreateRoute("price", "19.99");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("value");
        Assert.Equal("value", modelBindObject.Name);
        Assert.Equal(19.99f, (float)modelBindObject.Value, precision: 2);
        Assert.Equal(typeof(float), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_DateTimeParameter_ReturnsCorrectValue()
    {
        var template = CreateTemplate("/date/{when:datetime}");
        var expectedDate = DateTime.Parse("2026-10-30T12:00:00Z").ToUniversalTime();
        var route = CreateRoute("date", "2026-10-30T12:00:00Z");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("when");
        Assert.Equal("when", modelBindObject.Name);
        Assert.Equal(expectedDate, modelBindObject.Value);
        Assert.Equal(typeof(DateTime), modelBindObject.Type);
    }

    #endregion

    #region Multiple Parameters Binding

    [Fact]
    public void ModelBind_MultipleIntParameters_ReturnsAllValues()
    {
        var template = CreateTemplate("/foo/{a:int}/{b:int}");
        var route = CreateRoute("foo", "10", "20");

        var result = _binder.ModelBind(template, route);

        var a = result.GetValueByName("a");
        Assert.Equal("a", a.Name);
        Assert.Equal(10, a.Value);
        Assert.Equal(typeof(int), a.Type);

        var b = result.GetValueByName("b");
        Assert.Equal("b", b.Name);
        Assert.Equal(20, b.Value);
        Assert.Equal(typeof(int), b.Type);
    }

    [Fact]
    public void ModelBind_MixedTypeParameters_ReturnsCorrectTypes()
    {
        var template = CreateTemplate("/user/{id:int}/post/{guid:guid}");
        var expectedGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var route = CreateRoute("user", "123", "post", "550e8400-e29b-41d4-a716-446655440000");

        var result = _binder.ModelBind(template, route);

        var id = result.GetValueByName("id");
        Assert.Equal("id", id.Name);
        Assert.Equal(123, id.Value);
        Assert.Equal(typeof(int), id.Type);

        var guid = result.GetValueByName("guid");
        Assert.Equal("guid", guid.Name);
        Assert.Equal(expectedGuid, guid.Value);
        Assert.Equal(typeof(Guid), guid.Type);
    }

    [Fact]
    public void ModelBind_ThreeParameters_ReturnsAllValues()
    {
        var template = CreateTemplate("/a/{x:int}/b/{y:string}/c/{z:float}");
        var route = CreateRoute("a", "1", "b", "test", "c", "3.14");

        var result = _binder.ModelBind(template, route);

        var x = result.GetValueByName("x");
        Assert.Equal("x", x.Name);
        Assert.Equal(1, x.Value);
        Assert.Equal(typeof(int), x.Type);

        var y = result.GetValueByName("y");
        Assert.Equal("y", y.Name);
        Assert.Equal("test", y.Value);
        Assert.Equal(typeof(string), y.Type);

        var z = result.GetValueByName("z");
        Assert.Equal("z", z.Name);
        Assert.Equal(3.14f, (float)z.Value, precision: 2);
        Assert.Equal(typeof(float), z.Type);
    }

    #endregion

    #region Static and Dynamic Segments

    [Fact]
    public void ModelBind_StaticPrefix_IgnoresStaticSegments()
    {
        var template = CreateTemplate("/api/users/{id:int}");
        var route = CreateRoute("api", "users", "42");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("id");
        Assert.Equal("id", modelBindObject.Name);
        Assert.Equal(42, modelBindObject.Value);
        Assert.Equal(typeof(int), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_StaticBetweenDynamic_MapsCorrectly()
    {
        var template = CreateTemplate("/{id:int}/profile/{name:string}");
        var route = CreateRoute("123", "profile", "john");

        var result = _binder.ModelBind(template, route);

        var id = result.GetValueByName("id");
        Assert.Equal("id", id.Name);
        Assert.Equal(123, id.Value);
        Assert.Equal(typeof(int), id.Type);

        var name = result.GetValueByName("name");
        Assert.Equal("name", name.Name);
        Assert.Equal("john", name.Value);
        Assert.Equal(typeof(string), name.Type);
    }

    [Fact]
    public void ModelBind_MultipleStaticSegments_PositionsMatch()
    {
        var template = CreateTemplate("/a/b/{x:int}/c/d/{y:int}");
        var route = CreateRoute("a", "b", "10", "c", "d", "20");

        var result = _binder.ModelBind(template, route);

        var x = result.GetValueByName("x");
        Assert.Equal("x", x.Name);
        Assert.Equal(10, x.Value);
        Assert.Equal(typeof(int), x.Type);

        var y = result.GetValueByName("y");
        Assert.Equal("y", y.Name);
        Assert.Equal(20, y.Value);
        Assert.Equal(typeof(int), y.Type);
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

        var modelBindObject = result.GetValueByName("name");
        Assert.Equal("name", modelBindObject.Name);
        Assert.Equal("", modelBindObject.Value);
        Assert.Equal(typeof(string), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_SpecialCharactersInString_HandlesCorrectly()
    {
        var template = CreateTemplate("/{text:string}");
        var route = CreateRoute("hello-world_123");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("text");
        Assert.Equal("text", modelBindObject.Name);
        Assert.Equal("hello-world_123", modelBindObject.Value);
        Assert.Equal(typeof(string), modelBindObject.Type);
    }

    #endregion

    #region Type Conversion Edge Cases

    [Fact]
    public void ModelBind_IntMinValue_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:int}");
        var route = CreateRoute("-2147483648");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("value");
        Assert.Equal("value", modelBindObject.Name);
        Assert.Equal(int.MinValue, modelBindObject.Value);
        Assert.Equal(typeof(int), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_IntMaxValue_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:int}");
        var route = CreateRoute("2147483647");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("value");
        Assert.Equal("value", modelBindObject.Name);
        Assert.Equal(int.MaxValue, modelBindObject.Value);
        Assert.Equal(typeof(int), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_IntZero_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:int}");
        var route = CreateRoute("0");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("value");
        Assert.Equal("value", modelBindObject.Name);
        Assert.Equal(0, modelBindObject.Value);
        Assert.Equal(typeof(int), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_NegativeInt_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:int}");
        var route = CreateRoute("-123");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("value");
        Assert.Equal("value", modelBindObject.Name);
        Assert.Equal(-123, modelBindObject.Value);
        Assert.Equal(typeof(int), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_FloatZero_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:float}");
        var route = CreateRoute("0.0");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("value");
        Assert.Equal("value", modelBindObject.Name);
        Assert.Equal(0.0f, modelBindObject.Value);
        Assert.Equal(typeof(float), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_NegativeFloat_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:float}");
        var route = CreateRoute("-3.14");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("value");
        Assert.Equal("value", modelBindObject.Name);
        Assert.Equal(-3.14f, (float)modelBindObject.Value, precision: 2);
        Assert.Equal(typeof(float), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_VerySmallFloat_ParsesCorrectly()
    {
        var template = CreateTemplate("/{value:float}");
        var route = CreateRoute("0.00001");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("value");
        Assert.Equal("value", modelBindObject.Name);
        Assert.Equal(0.00001f, (float)modelBindObject.Value, precision: 5);
        Assert.Equal(typeof(float), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_GuidWithHyphens_ParsesCorrectly()
    {
        var template = CreateTemplate("/{id:guid}");
        var expectedGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var route = CreateRoute("550e8400-e29b-41d4-a716-446655440000");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("id");
        Assert.Equal("id", modelBindObject.Name);
        Assert.Equal(expectedGuid, modelBindObject.Value);
        Assert.Equal(typeof(Guid), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_DateTimeISO8601_ParsesCorrectly()
    {
        var template = CreateTemplate("/{when:datetime}");
        var expectedDate = DateTime.Parse("2026-10-30T10:30:00Z").ToUniversalTime();
        var route = CreateRoute("2026-10-30T10:30:00Z");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("when");
        Assert.Equal("when", modelBindObject.Name);
        Assert.Equal(expectedDate, modelBindObject.Value);
        Assert.Equal(typeof(DateTime), modelBindObject.Type);
    }

    [Fact]
    public void ModelBind_DateTimeWithoutTime_ParsesCorrectly()
    {
        var template = CreateTemplate("/{when:datetime}");
        var route = CreateRoute("2026-10-30");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("when");
        Assert.Equal("when", modelBindObject.Name);
        Assert.Equal(typeof(DateTime), modelBindObject.Type);

        var parsedDate = (DateTime)modelBindObject.Value;
        Assert.Equal(2026, parsedDate.Year);
        Assert.Equal(10, parsedDate.Month);
        Assert.Equal(30, parsedDate.Day);
    }

    #endregion

    #region Error Handling - Invalid Type Conversions

    [Fact]
    public void ModelBind_InvalidIntValue_ThrowsModelBindingException()
    {
        var template = CreateTemplate("/{id:int}");
        var route = CreateRoute("abc");

        var exception = Assert.Throws<ModelBindingException>(() => _binder.ModelBind(template, route));

        Assert.Contains("Cannot convert", exception.Message);
        Assert.Contains("'abc'", exception.Message);
        Assert.Contains("Int32", exception.Message);
        Assert.Contains("'id'", exception.Message);
    }

    [Fact]
    public void ModelBind_InvalidFloatValue_ThrowsModelBindingException()
    {
        var template = CreateTemplate("/{value:float}");
        var route = CreateRoute("not-a-number");

        var exception = Assert.Throws<ModelBindingException>(() => _binder.ModelBind(template, route));

        Assert.Contains("Cannot convert", exception.Message);
        Assert.Contains("'not-a-number'", exception.Message);
        Assert.Contains("Single", exception.Message);
        Assert.Contains("'value'", exception.Message);
    }

    [Fact]
    public void ModelBind_InvalidGuidValue_ThrowsModelBindingException()
    {
        var template = CreateTemplate("/{id:guid}");
        var route = CreateRoute("not-a-guid");

        var exception = Assert.Throws<ModelBindingException>(() => _binder.ModelBind(template, route));

        Assert.Contains("Cannot convert", exception.Message);
        Assert.Contains("'not-a-guid'", exception.Message);
        Assert.Contains("Guid", exception.Message);
        Assert.Contains("'id'", exception.Message);
    }

    [Fact]
    public void ModelBind_InvalidDateTimeValue_ThrowsModelBindingException()
    {
        var template = CreateTemplate("/{when:datetime}");
        var route = CreateRoute("invalid-date");

        var exception = Assert.Throws<ModelBindingException>(() => _binder.ModelBind(template, route));

        Assert.Contains("Cannot convert", exception.Message);
        Assert.Contains("'invalid-date'", exception.Message);
        Assert.Contains("DateTime", exception.Message);
        Assert.Contains("'when'", exception.Message);
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
    public void GetValueByName_ReturnsValueWithCorrectMetadata()
    {
        var template = CreateTemplate("/{id:int}");
        var route = CreateRoute("42");

        var result = _binder.ModelBind(template, route);

        var modelBindObject = result.GetValueByName("id");
        Assert.Equal("id", modelBindObject.Name);
        Assert.Equal(42, modelBindObject.Value);
        Assert.Equal(typeof(int), modelBindObject.Type);
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