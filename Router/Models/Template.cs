using System.Text.RegularExpressions;

namespace Router.Models;

public sealed class Template
{
    private readonly Regex regex;
    private readonly Dictionary<string, RouteVariable> variablesByName;
    private readonly RouteVariable[] variablesByPosition;

    public int CountVariables => variablesByPosition.Length;

    public Regex Regex => regex;

    public Template(
        Regex regex,
        IEnumerable<RouteVariable> variables)
    {
        this.regex = regex;

        variablesByPosition = variables.ToArray();

        variablesByName = variablesByPosition.ToDictionary(
            variable => variable.Name);

        //TODO проверка на order соответсвует variablesByPosition (в компляторе возможно провписать)
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