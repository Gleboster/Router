using Router.Models.Routes;
using Router.Models.Segments;
using Router.Models.Templates;
using Router.Models.Variables;

namespace Router.Components.Parsers;

internal class RouteParser
{
    public Route Parse(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route cannot be null or empty", nameof(route));

        return new Route(SegmentRoute(route));
    }

    public IEnumerable<RouteSegment> SegmentRoute(string route)
    {
        return route.Split("/", StringSplitOptions.RemoveEmptyEntries)
            .Select(val => new RouteSegment(val));
    }
}