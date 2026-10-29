
using Router.Models.Handlers;
using Router.Models.Variables;

namespace Router.Components.Compilers;

public class HandlerCompiler
{
    public HandlerMetadata Compile(Delegate action)
    {
        var variables = action
            .GetType()
            .GetMethod("Invoke")!
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
