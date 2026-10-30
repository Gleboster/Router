using Router.Components.Compilers;
using Router.Components.Execution;
using Router.DTOs;
using Router.DTOs.ModelBinding;
using Xunit;

namespace Router.Tests.Components.Execution;

public sealed class HandlerRunnerTests
{
    private readonly HandlerRunner _runner;
    private readonly HandlerMetadataParser _handlerParser;

    public HandlerRunnerTests()
    {
        _runner = new HandlerRunner();
        _handlerParser = new HandlerMetadataParser();
    }

    #region No Parameters

    [Fact]
    public void RunHandle_NoParameters_InvokesHandler()
    {
        // Arrange
        bool wasInvoked = false;
        Action handler = () => { wasInvoked = true; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = new ModelBindResult(Array.Empty<ModelBindObject>());

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.True(wasInvoked);
    }

    #endregion

    #region Single Parameter

    [Fact]
    public void RunHandle_IntParameter_PassesCorrectValue()
    {
        // Arrange
        int receivedValue = 0;
        Action<int> handler = (int id) => { receivedValue = id; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("id", typeof(int), 42));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(42, receivedValue);
    }

    [Fact]
    public void RunHandle_StringParameter_PassesCorrectValue()
    {
        // Arrange
        string receivedValue = null;
        Action<string> handler = (string name) => { receivedValue = name; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("name", typeof(string), "john"));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal("john", receivedValue);
    }

    [Fact]
    public void RunHandle_GuidParameter_PassesCorrectValue()
    {
        // Arrange
        Guid receivedValue = Guid.Empty;
        var expectedGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        Action<Guid> handler = (Guid id) => { receivedValue = id; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("id", typeof(Guid), expectedGuid));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(expectedGuid, receivedValue);
    }

    [Fact]
    public void RunHandle_FloatParameter_PassesCorrectValue()
    {
        // Arrange
        float receivedValue = 0f;
        Action<float> handler = (float value) => { receivedValue = value; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("value", typeof(float), 19.99f));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(19.99f, receivedValue, precision: 2);
    }

    [Fact]
    public void RunHandle_DateTimeParameter_PassesCorrectValue()
    {
        // Arrange
        DateTime receivedValue = DateTime.MinValue;
        var expectedDate = new DateTime(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);
        Action<DateTime> handler = (DateTime when) => { receivedValue = when; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("when", typeof(DateTime), expectedDate));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(expectedDate, receivedValue);
    }

    #endregion

    #region Multiple Parameters

    [Fact]
    public void RunHandle_TwoIntParameters_PassesCorrectValues()
    {
        // Arrange
        int receivedA = 0;
        int receivedB = 0;
        Action<int, int> handler = (int a, int b) =>
        {
            receivedA = a;
            receivedB = b;
        };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(
            ("a", typeof(int), 10),
            ("b", typeof(int), 20));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(10, receivedA);
        Assert.Equal(20, receivedB);
    }

    [Fact]
    public void RunHandle_MixedTypeParameters_PassesCorrectValues()
    {
        // Arrange
        int receivedId = 0;
        string receivedName = null;
        Action<int, string> handler = (int id, string name) =>
        {
            receivedId = id;
            receivedName = name;
        };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(
            ("id", typeof(int), 42),
            ("name", typeof(string), "test"));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(42, receivedId);
        Assert.Equal("test", receivedName);
    }

    [Fact]
    public void RunHandle_ThreeParameters_PassesCorrectValues()
    {
        // Arrange
        int receivedX = 0;
        string receivedY = null;
        float receivedZ = 0f;
        Action<int, string, float> handler = (int x, string y, float z) =>
        {
            receivedX = x;
            receivedY = y;
            receivedZ = z;
        };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(
            ("x", typeof(int), 1),
            ("y", typeof(string), "test"),
            ("z", typeof(float), 3.14f));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(1, receivedX);
        Assert.Equal("test", receivedY);
        Assert.Equal(3.14f, receivedZ, precision: 2);
    }

    #endregion

    #region Parameter Order

    [Fact]
    public void RunHandle_ParametersInDifferentOrder_MapsCorrectlyByName()
    {
        // Arrange
        int receivedFirst = 0;
        int receivedSecond = 0;
        Action<int, int> handler = (int first, int second) =>
        {
            receivedFirst = first;
            receivedSecond = second;
        };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(
            ("second", typeof(int), 200),
            ("first", typeof(int), 100));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(100, receivedFirst);
        Assert.Equal(200, receivedSecond);
    }

    [Fact]
    public void RunHandle_ComplexParameterOrder_MapsCorrectlyByName()
    {
        // Arrange
        Guid receivedUserId = Guid.Empty;
        int receivedPostId = 0;
        string receivedAction = null;
        var expectedGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

        Action<Guid, int, string> handler = (Guid userId, int postId, string action) =>
        {
            receivedUserId = userId;
            receivedPostId = postId;
            receivedAction = action;
        };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(
            ("action", typeof(string), "edit"),
            ("userId", typeof(Guid), expectedGuid),
            ("postId", typeof(int), 42));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(expectedGuid, receivedUserId);
        Assert.Equal(42, receivedPostId);
        Assert.Equal("edit", receivedAction);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void RunHandle_NullableParameter_PassesCorrectValue()
    {
        // Arrange
        int? receivedValue = null;
        Action<int?> handler = (int? id) => { receivedValue = id; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("id", typeof(int?), 42));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(42, receivedValue);
    }

    [Fact]
    public void RunHandle_EmptyStringParameter_PassesEmptyString()
    {
        // Arrange
        string receivedValue = null;
        Action<string> handler = (string text) => { receivedValue = text; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("text", typeof(string), ""));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal("", receivedValue);
    }

    [Fact]
    public void RunHandle_ZeroIntParameter_PassesZero()
    {
        // Arrange
        int receivedValue = -1;
        Action<int> handler = (int value) => { receivedValue = value; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("value", typeof(int), 0));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(0, receivedValue);
    }

    [Fact]
    public void RunHandle_NegativeIntParameter_PassesNegativeValue()
    {
        // Arrange
        int receivedValue = 0;
        Action<int> handler = (int value) => { receivedValue = value; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("value", typeof(int), -123));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(-123, receivedValue);
    }

    [Fact]
    public void RunHandle_MinMaxIntValues_PassesCorrectValues()
    {
        // Arrange
        int receivedMin = 0;
        int receivedMax = 0;
        Action<int, int> handler = (int min, int max) =>
        {
            receivedMin = min;
            receivedMax = max;
        };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(
            ("min", typeof(int), int.MinValue),
            ("max", typeof(int), int.MaxValue));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(int.MinValue, receivedMin);
        Assert.Equal(int.MaxValue, receivedMax);
    }

    #endregion

    #region Handler Execution

    [Fact]
    public void RunHandle_HandlerThrowsException_PropagatesException()
    {
        // Arrange
        Action<int> handler = (int id) => { throw new InvalidOperationException("Test exception"); };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("id", typeof(int), 42));

        // Act & Assert
        var exception = Assert.Throws<System.Reflection.TargetInvocationException>(
            () => _runner.RunHandle(metadata, bindResult));
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal("Test exception", exception.InnerException?.Message);
    }

    [Fact]
    public void RunHandle_HandlerModifiesExternalState_StateIsModified()
    {
        // Arrange
        var state = new List<int>();
        Action<int> handler = (int value) => { state.Add(value); };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = CreateBindResult(("value", typeof(int), 42));

        // Act
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Single(state);
        Assert.Equal(42, state[0]);
    }

    [Fact]
    public void RunHandle_CalledMultipleTimes_InvokesHandlerEachTime()
    {
        // Arrange
        int invocationCount = 0;
        Action handler = () => { invocationCount++; };
        var metadata = _handlerParser.Parse(handler);
        var bindResult = new ModelBindResult(Array.Empty<ModelBindObject>());

        // Act
        _runner.RunHandle(metadata, bindResult);
        _runner.RunHandle(metadata, bindResult);
        _runner.RunHandle(metadata, bindResult);

        // Assert
        Assert.Equal(3, invocationCount);
    }

    #endregion

    #region Helper Methods

    private ModelBindResult CreateBindResult(params (string name, Type type, object value)[] parameters)
    {
        var objects = parameters
            .Select(p => new ModelBindObject(p.name, p.type, p.value))
            .ToList();

        return new ModelBindResult(objects);
    }

    #endregion
}
