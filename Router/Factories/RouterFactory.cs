using Interfaces;
using Router.Components.Binding;
using Router.Components.Compilers;
using Router.Components.Execution;
using Router.Components.Parsers;
using Router.Components.Registry;
using Router.Components.Validation;

namespace Factories;

public class RouterFactory
{
    public IRouter CreateRouter()
    {
        var templateParser = new TemplateParser();
        var handlerMetadataParser = new HandlerMetadataParser();
        var routeParser = new RouteParser();
        var validatorRouteRegistration = new ValidatorRouteRegistration();
        var routeRegistry = new RouteRegistry();
        var modelBinder = new ModelBinder();
        var handlerRunner = new HandlerRunner();

        return new Router.Implementations.Router(
            templateParser,
            handlerMetadataParser,
            routeParser,
            validatorRouteRegistration,
            routeRegistry,
            modelBinder,
            handlerRunner
        );
    }
}
