using System.Reflection;

namespace Router.Models;


public sealed class Handler
{
    private readonly Delegate action;
    private readonly Dictionary<string, RouteVariable> variablesByName;
    private readonly RouteVariable[] variablesByPosition;

    public int CountParameters => variablesByPosition.Length;

    public Handler(
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
