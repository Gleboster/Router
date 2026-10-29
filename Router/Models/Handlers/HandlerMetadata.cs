using Router.Models.Variables;
using System.Reflection;

namespace Router.Models.Handlers;


public sealed class HandlerMetadata
{
    private readonly Delegate action;
    private readonly Dictionary<string, RouteVariable> variablesByName;
    private readonly RouteVariable[] variablesByPosition;

    public int CountParameters => variablesByPosition.Length;

    public HandlerMetadata(
        Delegate action,
        IEnumerable<RouteVariable> variables)
    {
        this.action = action;

        variablesByPosition = variables.ToArray();

        variablesByName = variablesByPosition.ToDictionary(
            variable => variable.Name);
    }

    public RouteVariable GetVariableByName(string name)
    {
        return variablesByName[name];
    }

    public RouteVariable GetVariableByOrder(int order)
    {
        return variablesByPosition[order];
    }
}
