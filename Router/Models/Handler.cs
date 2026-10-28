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
        //TODO Отдать на HandlerCompiler variablesByPosition = action
        //    .GetType()
        //    .GetMethod("Invoke")!
        //    .GetParameters()
        //    .Select(p => new RouteVariable(
        //        Name: p.Name, 
        //        Type: p.ParameterType, 
        //        Order: p.Position))
        //    .ToArray();

        //TODO проверка на order соответсвует variablesByPosition (в компляторе возможно провписать)

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
