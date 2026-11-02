using Router.Components.Registry.Trie;
using Router.Models.Registrations;
using Router.Models.Routes;

namespace Router.Components.Registry;

internal class RouteRegistry
{
    private readonly RouteTrie routeTrie;

    public RouteRegistry()
    {
        routeTrie = new RouteTrie();
    }

    public void Save(RouteRegistration registration)
    {
        routeTrie.Add(registration);
    }

    public RouteRegistration ResolveRoute(Route route)
    {
        return routeTrie.ResolveRoute(route);
    }
}