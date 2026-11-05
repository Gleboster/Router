using Router.Models.Variables;

namespace Router.Models.Segments;

internal record TemplateSegment() : Segment;
internal record StaticTemplateSegment(string Value) : TemplateSegment;
internal record DynamicTemplateSegment(RouteVariable RouteVariable) : TemplateSegment;