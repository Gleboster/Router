using Router.Components.Compilers;
using Router.Components.Validation;
using Router.Models.Registrations;
using Xunit;

namespace Router.Tests.Components.Validation;

public sealed class ValidatorRouteRegistrationTests
{
    private readonly ValidatorRouteRegistration _validator = new();
    private readonly TemplateParser _templateParser = new();
    private readonly HandlerMetadataParser _handlerParser = new();

    [Fact]
    public void ValidateParity_MatchingParametersCountAndTypes_DoesNotThrow()
    {
        var template = _templateParser.Parse("/users/{id:int}");
        Action<int> handler = (int id) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Record.Exception(() => _validator.ValidateParity(registration));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParity_MultipleMatchingParameters_DoesNotThrow()
    {
        var template = _templateParser.Parse("/users/{userId:int}/posts/{postId:guid}");
        Action<int, Guid> handler = (int userId, Guid postId) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Record.Exception(() => _validator.ValidateParity(registration));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParity_NoParameters_DoesNotThrow()
    {
        var template = _templateParser.Parse("/users/list");
        Action handler = () => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Record.Exception(() => _validator.ValidateParity(registration));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParity_ParameterCountMismatch_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/users/{id:int}");
        Action<int, string> handler = (int id, string name) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Parameter count mismatch", exception.Message);
        Assert.Contains("template has 1 parameters", exception.Message);
        Assert.Contains("handler has 2 parameters", exception.Message);
    }

    [Fact]
    public void ValidateParity_TemplateHasMoreParameters_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/users/{id:int}/posts/{postId:int}");
        Action<int> handler = (int id) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Parameter count mismatch", exception.Message);
        Assert.Contains("template has 2 parameters", exception.Message);
        Assert.Contains("handler has 1 parameters", exception.Message);
    }

    [Fact]
    public void ValidateParity_HandlerHasMoreParameters_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/users/{id:int}");
        Action<int, string, Guid> handler = (int id, string name, Guid guid) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Parameter count mismatch", exception.Message);
        Assert.Contains("template has 1 parameters", exception.Message);
        Assert.Contains("handler has 3 parameters", exception.Message);
    }

    [Fact]
    public void ValidateParity_TypeMismatch_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/users/{id:int}");
        Action<string> handler = (string id) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Type mismatch for parameter 'id'", exception.Message);
        Assert.Contains("template expects Int32", exception.Message);
        Assert.Contains("handler has String", exception.Message);
    }

    [Fact]
    public void ValidateParity_MultipleParametersWithTypeMismatch_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/users/{userId:int}/posts/{postId:guid}");
        Action<int, int> handler = (int userId, int postId) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Type mismatch for parameter 'postId'", exception.Message);
        Assert.Contains("template expects Guid", exception.Message);
        Assert.Contains("handler has Int32", exception.Message);
    }

    [Fact]
    public void ValidateParity_ParameterNameMismatch_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/users/{id:int}");
        Action<int> handler = (int userId) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Parameter 'userId' found in handler but not in template", exception.Message);
    }

    [Fact]
    public void ValidateParity_ComplexScenarioWithAllTypesMatching_DoesNotThrow()
    {
        var template = _templateParser.Parse("/api/{id:int}/{name:string}/{guid:guid}/{value:float}/{date:datetime}");
        Action<int, string, Guid, float, DateTime> handler =
            (int id, string name, Guid guid, float value, DateTime date) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Record.Exception(() => _validator.ValidateParity(registration));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParity_ParameterOrderDoesNotMatter_DoesNotThrow()
    {
        var template = _templateParser.Parse("/users/{id:int}/posts/{postId:guid}");
        Action<Guid, int> handler = (Guid postId, int id) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Record.Exception(() => _validator.ValidateParity(registration));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParity_FuncDelegate_ValidatesCorrectly()
    {
        var template = _templateParser.Parse("/users/{id:int}");
        Func<int, string> handler = (int id) => id.ToString();
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Record.Exception(() => _validator.ValidateParity(registration));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParity_GuidTypeMismatch_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/users/{id:guid}");
        Action<string> handler = (string id) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Type mismatch for parameter 'id'", exception.Message);
        Assert.Contains("template expects Guid", exception.Message);
        Assert.Contains("handler has String", exception.Message);
    }

    [Fact]
    public void ValidateParity_FloatTypeMismatch_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/items/{price:float}");
        Action<int> handler = (int price) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Type mismatch for parameter 'price'", exception.Message);
        Assert.Contains("template expects Single", exception.Message);
        Assert.Contains("handler has Int32", exception.Message);
    }

    [Fact]
    public void ValidateParity_DateTimeTypeMismatch_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/events/{date:datetime}");
        Action<string> handler = (string date) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Type mismatch for parameter 'date'", exception.Message);
        Assert.Contains("template expects DateTime", exception.Message);
        Assert.Contains("handler has String", exception.Message);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(float))]
    [InlineData(typeof(DateTime))]
    public void ValidateParity_AllSupportedTypesMatching_DoesNotThrow(Type parameterType)
    {
        var typeString = parameterType.Name.ToLower();
        if (typeString == "int32") typeString = "int";
        if (typeString == "single") typeString = "float";

        var template = _templateParser.Parse($"/route/{{param:{typeString}}}");

        var handlerType = typeof(Action<>).MakeGenericType(parameterType);
        var method = GetType().GetMethod(
            nameof(DummyHandler),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        ) ?? throw new InvalidOperationException("DummyHandler not found");

        method = method.MakeGenericMethod(parameterType);
        var handler = Delegate.CreateDelegate(handlerType, this, method);
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Record.Exception(() => _validator.ValidateParity(registration));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParity_MultipleParametersWithOneNameMismatch_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/users/{id:int}/posts/{postId:int}");
        Action<int, int> handler = (int id, int differentPostId) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Parameter 'differentPostId' found in handler but not in template", exception.Message);
    }

    [Fact]
    public void ValidateParity_CaseSensitiveParameterNames_ThrowsInvalidOperationException()
    {
        var template = _templateParser.Parse("/users/{id:int}");
        Action<int> handler = (int Id) => { };
        var handlerMetadata = _handlerParser.Parse(handler);

        var registration = new RouteRegistration(template, handlerMetadata);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _validator.ValidateParity(registration));

        Assert.Contains("Parameter 'Id' found in handler but not in template", exception.Message);
    }

    private void DummyHandler<T>(T param) { }
}
