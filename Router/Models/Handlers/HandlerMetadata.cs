using Router.Models.Variables;
using System.Collections.Frozen;
using System.Reflection;

namespace Router.Models.Handlers;


public sealed class HandlerMetadata
{
    private readonly Delegate action;
    private readonly FrozenDictionary<string, RouteVariable> variablesByName;

    public int CountParameters => variablesByName.Count;
    public Delegate Action => action;

    public HandlerMetadata(
        Delegate action,
        IEnumerable<RouteVariable> variables)
    {
        this.action = action;

        variablesByName = variables.ToFrozenDictionary(
            variable => variable.Name);
    }

    public IReadOnlyCollection<RouteVariable> GetVariables() 
        => variablesByName.Values;

    public RouteVariable GetVariableByName(string name) 
        => variablesByName[name];
}
