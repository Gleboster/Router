using Router.Models.Handlers;
using Router.Models.Registrations;
using Router.Models.Templates;

namespace Router.Components.Validation;

public class ValidatorRouteRegistration
{
    public void ValidateParity(RouteRegistration registration)
    {
        var template = registration.Template;
        var handler = registration.Handler;

        CheckCountVariables(template, handler);
        CheckTypeNameVariables(template, handler);
    }

    private static void CheckTypeNameVariables(
        Template template, 
        HandlerMetadata handler)
    {
        var handlerVariables = handler.GetVariables();

        foreach (var handlerVariable in handlerVariables)
        {
            try
            {
                var templateVariable = template.GetVariableByName(handlerVariable.Name);

                if (templateVariable.Type != handlerVariable.Type)
                {
                    throw new InvalidOperationException(
                        $"Type mismatch for parameter '{handlerVariable.Name}': " +
                        $"template expects {templateVariable.Type.Name}, " +
                        $"handler has {handlerVariable.Type.Name}");
                }
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(
                    $"Parameter '{handlerVariable.Name}' found in handler but not in template");
            }
        }
    }

    private static void CheckCountVariables(
        Template template,
        HandlerMetadata handler)
    {
        if (template.CountVariables != handler.CountParameters)
        {
            throw new InvalidOperationException(
                $"Parameter count mismatch: template has {template.CountVariables} parameters, " +
                $"handler has {handler.CountParameters} parameters");
        }
    }
}
