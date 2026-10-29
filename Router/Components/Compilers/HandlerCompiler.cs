
using Router.Models;
using Router.Models.Handlers;

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
                Type: p.ParameterType,
                Order: p.Position)
            );

        return new HandlerMetadata(
            action,
            variables
        );
    }
}
