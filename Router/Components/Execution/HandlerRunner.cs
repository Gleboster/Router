using System.Reflection;
using Router.DTOs.ModelBinding;
using Router.Exceptions;
using Router.Models.Handlers;
using Router.Models.Variables;

namespace Router.Components.Execution;

internal class HandlerRunner
{
    public void RunHandle(HandlerMetadata handlerMetadata, ModelBindResult modelBindResult)
    {
        var args = GetArgs(handlerMetadata, modelBindResult);

        try
        {
            handlerMetadata.Action.DynamicInvoke(args);
        }
        catch (TargetInvocationException ex)
        {
            throw new HandlerExecutionException(
                $"Handler execution failed: {ex.InnerException?.Message ?? ex.Message}",
                ex.InnerException ?? ex);
        }
    }

    public async Task RunHandleAsync(
        HandlerMetadata handlerMetadata,
        ModelBindResult modelBindResult,
        CancellationToken cancellationToken)
    {
        var args = GetArgs(handlerMetadata, modelBindResult);

        try
        {
            await Task.Run(() => handlerMetadata.Action.DynamicInvoke(args), cancellationToken);
        }
        catch (TargetInvocationException ex)
        {
            throw new HandlerExecutionException(
                $"Handler execution failed: {ex.InnerException?.Message ?? ex.Message}",
                ex.InnerException ?? ex);
        }
    }

    private object[] GetArgs(HandlerMetadata handlerMetadata, ModelBindResult modelBindResult)
    {
        var handlerVariables = handlerMetadata.GetVariables();

        List<object> args = new List<object>();

        foreach (var variable in handlerVariables)
        {
            object arg = GetValueByNameWithType(modelBindResult, variable);
            args.Add(arg);
        }

        return args.ToArray();
    }

    private object GetValueByNameWithType(
        ModelBindResult modelBindResult,
        RouteVariable variable)
    {
        var modelBindObject = modelBindResult.GetValueByName(variable.Name);

        if (modelBindObject == null)
            throw new ModelBindingException(
                $"Parameter '{variable.Name}' not found in model binding results");

        if (modelBindObject.Type != variable.Type)
            throw new ModelBindingException(
                $"Type mismatch for parameter '{variable.Name}': " +
                $"expected {variable.Type.Name}, got {modelBindObject.Type.Name}");

        return modelBindObject.Value;
    }
}