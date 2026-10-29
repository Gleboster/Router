using Router.Models.Segments;
using Router.Models.Variables;
using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Router.Models.Templates;

public sealed class Template
{
    private readonly FrozenDictionary<string, RouteVariable> variablesByName;

    private readonly ReadOnlyCollection<Segment> segments;

    public IReadOnlyList<Segment> Segments => segments;


    public int CountVariables => variablesByName.Count;

    public Template(
        IEnumerable<Segment> segments, 
        IEnumerable<RouteVariable> variables)
    {
        this.segments = segments.ToList().AsReadOnly();

        variablesByName = variables.ToFrozenDictionary(
            variable => variable.Name);
    }

    public RouteVariable GetVariableByName(string name)
        => variablesByName[name];
}