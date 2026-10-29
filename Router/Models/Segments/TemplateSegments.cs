using Router.Models.Variables;

namespace Router.Models.Segments;

public record TemplateSegment() : Segment;
public record StaticTemplateSegment(string Value) : TemplateSegment;
public record DynamicTemplateSegment(RouteVariable RouteVariable) : TemplateSegment;