using Router.Components.Compilers;
using Router.Models.Variables;
using System.Reflection;
using Xunit;

namespace Router.Tests.Components.Parsers;

public sealed class HandlerMetadataParserTests
{
    private readonly HandlerMetadataParser _parser = new();

    #region Parameter Count Tests

    [Fact]
    public void Parse_ActionWithNoParameters_ReturnsZeroVariables()
    {
        Action handler = () => { };

        var metadata = _parser.Parse(handler);

        Assert.Equal(0, metadata.CountParameters);
    }

    [Fact]
    public void Parse_ActionWithOneParameter_ReturnsOneVariable()
    {
        Action<int> handler = (int id) => { };

        var metadata = _parser.Parse(handler);

        Assert.Equal(1, metadata.CountParameters);
    }

    [Fact]
    public void Parse_ActionWithTwoParameters_ReturnsTwoVariables()
    {
        Action<int, string> handler = (int id, string name) => { };

        var metadata = _parser.Parse(handler);

        Assert.Equal(2, metadata.CountParameters);
    }

    #endregion

    #region Variable Creation Tests

    [Fact]
    public void Parse_ActionWithIntParameter_CreatesCorrectVariable()
    {
        Action<int> handler = (int id) => { };

        var metadata = _parser.Parse(handler);
        var variable = metadata.GetVariableByName("id");

        Assert.Equal("id", variable.Name);
        Assert.Equal(typeof(int), variable.Type);
    }

    [Fact]
    public void Parse_ActionWithMultipleParameters_CreatesCorrectVariables()
    {
        Action<int, string, Guid> handler = (int userId, string name, Guid sessionId) => { };

        var metadata = _parser.Parse(handler);

        var userIdVar = metadata.GetVariableByName("userId");
        var nameVar = metadata.GetVariableByName("name");
        var sessionIdVar = metadata.GetVariableByName("sessionId");

        Assert.Equal("userId", userIdVar.Name);
        Assert.Equal(typeof(int), userIdVar.Type);

        Assert.Equal("name", nameVar.Name);
        Assert.Equal(typeof(string), nameVar.Type);

        Assert.Equal("sessionId", sessionIdVar.Name);
        Assert.Equal(typeof(Guid), sessionIdVar.Type);
    }

    #endregion

    #region Type Handling Tests

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(float))]
    [InlineData(typeof(DateTime))]
    public void Parse_ActionWithDifferentTypes_ParsesCorrectly(Type parameterType)
    {
        var handlerType = typeof(Action<>).MakeGenericType(parameterType);

        var method = GetType().GetMethod(
            nameof(DummyMethod),
            BindingFlags.Instance | BindingFlags.NonPublic
        ) ?? throw new InvalidOperationException("DummyMethod not found");

        method = method.MakeGenericMethod(parameterType);

        var handler = Delegate.CreateDelegate(handlerType, this, method);

        var metadata = _parser.Parse(handler);

        Assert.Equal(1, metadata.CountParameters);

        var variables = metadata.GetVariables();
        var variable = variables.First();

        Assert.Equal(parameterType, variable.Type);
    }

    #endregion

    #region GetVariables Tests

    [Fact]
    public void Parse_GetVariables_ReturnsAllVariables()
    {
        Action<int, string> handler = (int id, string name) => { };

        var metadata = _parser.Parse(handler);
        var variables = metadata.GetVariables().ToList();

        Assert.Equal(2, variables.Count);
        Assert.Contains(variables, v => v.Name == "id" && v.Type == typeof(int));
        Assert.Contains(variables, v => v.Name == "name" && v.Type == typeof(string));
    }

    [Fact]
    public void Parse_ParameterOrder_PreservesOrder()
    {
        Action<int, string, Guid> handler = (int first, string second, Guid third) => { };

        var metadata = _parser.Parse(handler);
        var variables = metadata.GetVariables().ToList();

        Assert.Equal("first", variables[0].Name);
        Assert.Equal("second", variables[1].Name);
        Assert.Equal("third", variables[2].Name);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Parse_GetVariableByName_ThrowsWhenNotFound()
    {
        Action<int> handler = (int id) => { };

        var metadata = _parser.Parse(handler);

        Assert.Throws<KeyNotFoundException>(() =>
            metadata.GetVariableByName("nonexistent"));
    }

    #endregion

    #region Different Delegate Types Tests

    [Fact]
    public void Parse_FuncDelegate_ParsesCorrectly()
    {
        Func<int, string> handler = (int id) => id.ToString();

        var metadata = _parser.Parse(handler);

        Assert.Equal(1, metadata.CountParameters);

        var variable = metadata.GetVariableByName("id");
        Assert.Equal("id", variable.Name);
        Assert.Equal(typeof(int), variable.Type);
    }

    [Fact]
    public void Parse_CustomDelegate_ParsesCorrectly()
    {
        CustomHandler handler = (int a, string b) => { };

        var metadata = _parser.Parse(handler);

        Assert.Equal(2, metadata.CountParameters);

        var aVar = metadata.GetVariableByName("a");
        var bVar = metadata.GetVariableByName("b");

        Assert.Equal(typeof(int), aVar.Type);
        Assert.Equal(typeof(string), bVar.Type);
    }

    [Fact]
    public void Parse_InnerFuncWithIntParameter_CreatesCorrectMetadata()
    {
        void foo(int id)
        {

        }

        Action<int> handler = foo;

        var metadata = _parser.Parse(handler);
        var variable = metadata.GetVariableByName("id");

        Assert.Equal(1, metadata.CountParameters);
        Assert.Equal("id", variable.Name);
        Assert.Equal(typeof(int), variable.Type);
    }

    #endregion

    #region Helper Methods

    private void DummyMethod<T>(T param) { }

    private delegate void CustomHandler(int a, string b);

    #endregion
}
