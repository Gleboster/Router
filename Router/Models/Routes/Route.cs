using Router.Models.Segments;
using System.Collections.ObjectModel;

namespace Router.Models.Routes;

public sealed class Route
{
    private readonly ReadOnlyCollection<RouteSegment> segments;
    public IReadOnlyList<RouteSegment> Segments => segments;

    public Route(IEnumerable<RouteSegment> segments)
    {
        this.segments = segments.ToList().AsReadOnly();
    }
}