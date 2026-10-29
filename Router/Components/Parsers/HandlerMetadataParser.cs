
using Router.Models.Handlers;
using Router.Models.Variables;

namespace Router.Components.Compilers;

public class HandlerMetadataParser
{
    public HandlerMetadata Parse(Delegate action)
    {
        var variables = action
            .Method
            .GetParameters()
            .Select(p => new RouteVariable(
                Name: p.Name,
                Type: p.ParameterType)
            );

        return new HandlerMetadata(
            action,
            variables
        );
    }
}
