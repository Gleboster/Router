using Router.Models.Variables;
using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Router.Models.Templates;

public sealed class Template
{
    private readonly Regex regex;
    private readonly FrozenDictionary<string, RouteVariable> variablesByName;

    public int CountVariables => variablesByName.Count;

    public Regex Regex => regex;

    public Template(
        Regex regex,
        IEnumerable<RouteVariable> variables)
    {
        this.regex = regex;

        variablesByName = variables.ToFrozenDictionary(
            variable => variable.Name);
    }

    public RouteVariable GetVariableByName(string name) 
        => variablesByName[name];
}