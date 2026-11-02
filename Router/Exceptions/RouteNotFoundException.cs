using Router.Models.Routes;

namespace Router.Exceptions;

public class RouteNotFoundException : Exception
{
    public string Route { get; }

    internal RouteNotFoundException(Route route)
    : this(route.ToString())
    {

    }

    internal RouteNotFoundException(string route)
        : base($"No route found for path: {route}")
    {
        Route = route;
    }
}
