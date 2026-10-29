using Router.Components.Compilers;
using Router.Models.Segments;
using Router.Models.Variables;
using Xunit;

namespace Router.Tests.Components.Parsers;

public sealed class TemplateParserTests
{
    private readonly TemplateParser _parser = new();

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

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Parse_EmptyTemplate_ThrowsArgumentException(string template)
    {
        var exception = Assert.Throws<ArgumentException>(() => _parser.Parse(template));
        Assert.Equal("template", exception.ParamName);
    }

    [Fact]
    public void Parse_UnknownType_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            _parser.Parse("/route/{param:unknown}"));

        Assert.Contains("unknown", exception.Message);
        Assert.Contains("param", exception.Message);
        Assert.Equal("template", exception.ParamName);
    }

    [Fact]
    public void SegmentTemplate_ReturnsSegmentsAndVariables()
    {
        var (segments, variables) = _parser.SegmentTemplate("/foo/{id:int}/bar");

        var segmentList = segments.ToList();
        var variableList = variables.ToList();

        Assert.Equal(3, segmentList.Count);
        Assert.Single(variableList);

        Assert.Equal("id", variableList[0].Name);
        Assert.Equal(typeof(int), variableList[0].Type);
    }

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
}
