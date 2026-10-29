using Router.Models.Routes;
using Router.Models.Segments;
using Router.Models.Templates;
using Router.Models.Variables;

namespace Router.Components.Parsers;

public class RouteParser
{
    public Route Parse(string route)
    {
        return new Route(SegmentRoute(route));
    }

    public IEnumerable<RouteSegment> SegmentRoute(string route)
    {
        return route.Split("/", StringSplitOptions.RemoveEmptyEntries)
            .Select(val => new RouteSegment(val));
    }
}