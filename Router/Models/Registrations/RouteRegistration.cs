using Router.Models.Handlers;
using Router.Models.Templates;

namespace Router.Models.Registrations;

public class RouteRegistration(Template template, HandlerMetadata handler)
{
    public Template Template { get; } = template;
    public HandlerMetadata Handler { get; } = handler;
}
