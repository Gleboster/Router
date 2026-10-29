using Router.Models.Registrations;
using Router.Models.Routes;
using Router.Models.Segments;

namespace Router.Components.Registry.Trie;

public class RouteTrie
{
    private readonly SegmentNode rootNode;

    public RouteTrie()
    {
        rootNode = new SegmentNode();
    }

    public void Add(RouteRegistration routeRegistration)
    {

    }

    public RouteRegistration ResolveRoute(Route route)
    {
        throw new NotImplementedException();
    }

    public bool ResolveSegment( //TODO нужно получать результат о метчинге пока хз какой
        TemplateSegment templateSegment,
        RouteSegment routeSegment)
    {
        throw new NotImplementedException();
    }
}