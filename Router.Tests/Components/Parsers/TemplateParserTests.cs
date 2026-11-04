using Router.Components.Compilers;
using Router.Exceptions;
using Router.Models.Segments;
using Router.Models.Variables;
using Xunit;

namespace Router.Tests.Components.Parsers;

public class TemplateParserTests
{
    private readonly TemplateParser _parser = new();

    #region Parsing Static Segments

    [Fact]
    public void Parse_StaticSegments_ReturnsCorrectSegments()
    {
        var template = _parser.Parse("/foo/bar/baz");

        Assert.Equal(3, template.Segments.Count);
        Assert.All(template.Segments, s => Assert.IsType<StaticTemplateSegment>(s));

        var segments = template.Segments.Cast<StaticTemplateSegment>().ToList();
        Assert.Equal("foo", segments[0].Value);
        Assert.Equal("bar", segments[1].Value);
        Assert.Equal("baz", segments[2].Value);
    }

    #endregion

    #region Parsing Dynamic Segments

    [Fact]
    public void Parse_DynamicSegments_ReturnsCorrectSegments()
    {
        var template = _parser.Parse("/users/{id:int}/posts/{postId:guid}");

        Assert.Equal(4, template.Segments.Count);

        Assert.IsType<StaticTemplateSegment>(template.Segments[0]);
        Assert.IsType<DynamicTemplateSegment>(template.Segments[1]);
        Assert.IsType<StaticTemplateSegment>(template.Segments[2]);
        Assert.IsType<DynamicTemplateSegment>(template.Segments[3]);
    }

    [Fact]
    public void Parse_DynamicSegment_CreatesCorrectVariable()
    {
        var template = _parser.Parse("/users/{id:int}");

        var dynamicSegment = template.Segments[1] as DynamicTemplateSegment;

        Assert.NotNull(dynamicSegment);
        Assert.Equal("id", dynamicSegment.RouteVariable.Name);
        Assert.Equal(typeof(int), dynamicSegment.RouteVariable.Type);
    }

    [Theory]
    [InlineData("string", typeof(string))]
    [InlineData("int", typeof(int))]
    [InlineData("guid", typeof(Guid))]
    [InlineData("float", typeof(float))]
    [InlineData("datetime", typeof(DateTime))]
    public void Parse_AllSupportedTypes_ParsesCorrectly(string typeStr, Type expectedType)
    {
        var template = _parser.Parse($"/route/{{param:{typeStr}}}");

        var dynamicSegment = template.Segments[1] as DynamicTemplateSegment;

        Assert.NotNull(dynamicSegment);
        Assert.Equal("param", dynamicSegment.RouteVariable.Name);
        Assert.Equal(expectedType, dynamicSegment.RouteVariable.Type);
    }

    #endregion

    #region Variable Handling

    [Fact]
    public void Parse_MultipleParameters_ReturnsCorrectVariableCount()
    {
        var template = _parser.Parse("/foo/{a:int}/bar/{b:string}");

        Assert.Equal(2, template.CountVariables);
    }

    [Fact]
    public void Parse_NoParameters_ReturnsZeroVariables()
    {
        var template = _parser.Parse("/foo/bar/baz");

        Assert.Equal(0, template.CountVariables);
    }

    [Fact]
    public void Parse_GetVariableByName_ReturnsCorrectVariable()
    {
        var template = _parser.Parse("/users/{id:int}/posts/{postId:guid}");

        var idVariable = template.GetVariableByName("id");
        var postIdVariable = template.GetVariableByName("postId");

        Assert.Equal("id", idVariable.Name);
        Assert.Equal(typeof(int), idVariable.Type);
        Assert.Equal("postId", postIdVariable.Name);
        Assert.Equal(typeof(Guid), postIdVariable.Type);
    }

    #endregion

    #region Error Handling

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Parse_EmptyTemplate_ThrowsArgumentException(string template)
    {
        var exception = Assert.ThrowsAny<ArgumentException>(() => _parser.Parse(template));
    }

    [Fact]
    public void Parse_UnknownType_ThrowsInvalidTemplateFormatException()
    {
        var exception = Assert.Throws<InvalidTemplateFormatException>(() =>
            _parser.Parse("/route/{param:unknown}"));

        Assert.Contains("unknown", exception.Message);
        Assert.Contains("param", exception.Message);
        Assert.Equal("template", exception.ParamName);
    }

    [Theory]
    [InlineData("/route/{unclosed")]
    [InlineData("/route/unclosed}")]
    [InlineData("/route/{{nested:int}}")]
    public void Parse_UnclosedBraces_ThrowsInvalidTemplateFormatException(string template)
    {
        var exception = Assert.Throws<InvalidTemplateFormatException>(() =>
            _parser.Parse(template));

        Assert.Contains("Invalid parameter format", exception.Message);
        Assert.Equal("template", exception.ParamName);
    }

    [Fact]
    public void Parse_ParameterWithoutType_ThrowsInvalidTemplateFormatException()
    {
        var exception = Assert.Throws<InvalidTemplateFormatException>(() =>
            _parser.Parse("/route/{id}"));

        Assert.Contains("Invalid parameter format", exception.Message);
        Assert.Contains("{id}", exception.Message);
        Assert.Equal("template", exception.ParamName);
    }

    [Fact]
    public void Parse_EmptyParameterName_ThrowsInvalidTemplateFormatException()
    {
        var exception = Assert.Throws<InvalidTemplateFormatException>(() =>
            _parser.Parse("/route/{:int}"));

        Assert.Contains("Invalid parameter format in segment", exception.Message);
        Assert.Equal("template", exception.ParamName);
    }

    [Fact]
    public void Parse_DuplicateParameterNames_ThrowsInvalidTemplateFormatException()
    {
        var exception = Assert.Throws<InvalidTemplateFormatException>(() =>
            _parser.Parse("/route/{id:int}/users/{id:string}"));

        Assert.Contains("Duplicate parameter name", exception.Message);
        Assert.Contains("id", exception.Message);
        Assert.Equal("template", exception.ParamName);
    }

    #endregion

    #region Slash Handling

    [Fact]
    public void Parse_TrailingSlash_ParsesCorrectly()
    {
        var template = _parser.Parse("/foo/bar/");

        Assert.Equal(2, template.Segments.Count);
    }

    [Fact]
    public void Parse_LeadingSlash_ParsesCorrectly()
    {
        var template = _parser.Parse("/foo/bar");

        Assert.Equal(2, template.Segments.Count);
    }

    [Fact]
    public void Parse_NoLeadingSlash_ParsesCorrectly()
    {
        var template = _parser.Parse("foo/bar");

        Assert.Equal(2, template.Segments.Count);
    }

    #endregion

    #region Complex Templates

    [Fact]
    public void Parse_ComplexTemplate_ParsesCorrectly()
    {
        var template = _parser.Parse("/api/v1/users/{userId:guid}/posts/{postId:int}/comments/{commentId:int}");

        Assert.Equal(8, template.Segments.Count);
        Assert.Equal(3, template.CountVariables);

        var userIdVar = template.GetVariableByName("userId");
        var postIdVar = template.GetVariableByName("postId");
        var commentIdVar = template.GetVariableByName("commentId");

        Assert.Equal(typeof(Guid), userIdVar.Type);
        Assert.Equal(typeof(int), postIdVar.Type);
        Assert.Equal(typeof(int), commentIdVar.Type);
    }

    #endregion
}
