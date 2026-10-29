using Router.Models.Segments;
using Router.Models.Variables;
using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Router.Models.Templates;

public sealed class Template
{
    private readonly FrozenDictionary<string, RouteVariable> variablesByName;

    private readonly ReadOnlyCollection<TemplateSegment> segments;

    public IReadOnlyList<TemplateSegment> Segments => segments;


    public int CountVariables => variablesByName.Count;

    public Template(
        IEnumerable<TemplateSegment> segments, 
        IEnumerable<RouteVariable> variables)
    {
        this.segments = segments.ToList().AsReadOnly();

        variablesByName = variables.ToFrozenDictionary(
            variable => variable.Name);
    }

    public RouteVariable GetVariableByName(string name)
        => variablesByName[name];
}