using Router.Models.Routes;
using Router.Models.Segments;
using Router.Models.Templates;
using Router.Models.Variables;

namespace Router.Components.Parsers;

internal class RouteParser
{
    public Route Parse(string route)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(route, nameof(route));

        return new Route(SegmentRoute(route));
    }

    private IEnumerable<RouteSegment> SegmentRoute(string route)
    {
        return route.Split("/", StringSplitOptions.RemoveEmptyEntries)
            .Select(val => new RouteSegment(val));
    }
}