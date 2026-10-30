using Interfaces;
using Router.Components.Binding;
using Router.Components.Compilers;
using Router.Components.Execution;
using Router.Components.Parsers;
using Router.Components.Registry;
using Router.Components.Validation;
using Router.Models.Registrations;
using System.Threading.Tasks;

namespace Router.Implementations;

public class Router : IRouter
{
    private readonly TemplateParser templateParser;
    private readonly HandlerMetadataParser handlerMetadataParser;

    private readonly RouteParser routeParser;


    private readonly ValidatorRouteRegistration validatorRouteRegistration;

    private readonly RouteRegistry routeRegistry;

    private readonly ModelBinder modelBinder;

    private readonly HandlerRunner handlerRunner;

    public Router(TemplateParser templateParser, HandlerMetadataParser handlerMetadataParser, RouteParser routeParser, ValidatorRouteRegistration validatorRouteRegistration, RouteRegistry routeRegistry, ModelBinder modelBinder, HandlerRunner handlerRunner)
    {
        this.templateParser = templateParser;
        this.handlerMetadataParser = handlerMetadataParser;
        this.routeParser = routeParser;
        this.validatorRouteRegistration = validatorRouteRegistration;
        this.routeRegistry = routeRegistry;
        this.modelBinder = modelBinder;
        this.handlerRunner = handlerRunner;
    }


    public void RegisterRoute(string template, Action action)
    {
        RegistrationRoute(template, action);
    }

    public void RegisterRoute<T1>(string template, Action<T1> action)
    {
        RegistrationRoute(template, action);
    }

    public void RegisterRoute<T1, T2>(string template, Action<T1, T2> action)
    {
        RegistrationRoute(template, action);
    }

    public void Route(string route)
    {
        var routeObj = routeParser.Parse(route);

        var routeRegistartion = routeRegistry.ResolveRoute(routeObj);

        var templateObj = routeRegistartion.Template;
        var handlerObj = routeRegistartion.Handler;

        var modelBindResult = modelBinder.ModelBind(templateObj, routeObj);

        handlerRunner.RunHandle(handlerObj, modelBindResult);
    }

    public async Task RouteAsync(string route)
    {
        var routeObj = routeParser.Parse(route);

        var routeRegistartion = routeRegistry.ResolveRoute(routeObj);

        var templateObj = routeRegistartion.Template;
        var handlerObj = routeRegistartion.Handler;

        var modelBindResult = modelBinder.ModelBind(templateObj, routeObj);

        await handlerRunner.RunHandleAsync(handlerObj, modelBindResult);
    }

    private void RegistrationRoute(string template, Delegate action)
    {
        var templateObj = templateParser.Parse(template);
        var handlerObj = handlerMetadataParser.Parse(action);

        var routeRegistartion = new RouteRegistration(templateObj, handlerObj);

        validatorRouteRegistration.ValidateParity(routeRegistartion);

        routeRegistry.Save(routeRegistartion);
    }
}
