using Router.Components.Registry.Trie;
using Router.DTOs.Results;
using Router.Exceptions;
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
        var result = routeTrie.Add(registration);

        if (result.Status == AddRouteStatus.Duplicate)
            throw new DuplicateRouteException(
                registration.Template.ToString(),
                $"Route '{registration.Template}' is already registered. " +
                $"Cannot register duplicate routes with the same structure.");
    }

    public RouteRegistration ResolveRoute(Route route)
    {
        var result = routeTrie.ResolveRoute(route);

        if (result == null)
            throw new RouteNotFoundException(route);

        return result;
    }
}