using Router.DTOs.ModelBinding;
using Router.Models.Handlers;

namespace Router.Components.Execution;

public class HandlerRunner
{
    public void RunHandle(HandlerMetadata handlerMetadata, ModelBindResult modelBindResult)
    {
        var args = GetArgs(handlerMetadata, modelBindResult);
        handlerMetadata.Action.DynamicInvoke(args);
    }

    private object[] GetArgs(HandlerMetadata handlerMetadata, ModelBindResult modelBindResult)
    {
        var handlerVariables = handlerMetadata.GetVariables();

        List<object> args = new List<object>();

        foreach (var variable in handlerVariables)
        {
            var arg = modelBindResult.GetValueByNameWithType(variable.Name, variable.Type);   
            args.Add(arg);
        }

        return args.ToArray();
    }
}