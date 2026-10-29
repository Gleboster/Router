using Router.Models.Variables;

namespace Router.Models.Segments;

public record StaticTemplateSegment(string Value) : Segment;
public record DynamicTemplateSegment(RouteVariable RouteVariable) : Segment;