
using Router.Exceptions;
using Router.Models.Handlers;
using Router.Models.Variables;

namespace Router.Components.Compilers;

internal class HandlerMetadataParser
{
    public HandlerMetadata Parse(Delegate action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var parameters = action.Method.GetParameters();
        var variables = new List<RouteVariable>();

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (string.IsNullOrWhiteSpace(parameter.Name) ||
                parameter.Name == "_")
                throw new InvalidHandlerSignatureException(
                    $"Handler parameter at position {i} has no valid name");

            variables.Add(new RouteVariable(
                Name: parameter.Name,
                Type: parameter.ParameterType)
            );
        }

        return new HandlerMetadata(
            action,
            variables
        );
    }
}
