
using Router.Models;

namespace Router.Components.Compilers;

public class HandlerCompiler
{
    public Handler Compile(Delegate action)
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

        return new Handler(
            action,
            variables
        );
    }
}
