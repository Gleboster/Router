using Router.Models.Segments;
using Router.Models.Templates;
using Router.Models.Variables;
using System.Text.RegularExpressions;

namespace Router.Components.Compilers;

public class TemplateParser
{
    private static readonly Regex SegmentRegex = new(
        @"\{(?<name>[^:}]+):(?<type>[^}]+)\}",
        RegexOptions.Compiled);

    private static readonly Dictionary<string, Type> TypeMap = new()
    {
        ["string"] = typeof(string),
        ["int"] = typeof(int),
        ["guid"] = typeof(Guid),
        ["float"] = typeof(float),
        ["datetime"] = typeof(DateTime)
    };

    public Template Parse(string template)
    {
        var (segments, variables) = SegmentTemplate(template);
        return new Template(segments, variables);
    }

    private (IEnumerable<TemplateSegment>, IEnumerable<RouteVariable>) SegmentTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Template cannot be empty", nameof(template));

        var segments = new List<TemplateSegment>();
        var variables = new List<RouteVariable>();

        var parts = template.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var match = SegmentRegex.Match(part);

            if (match.Success)
            {
                var name = match.Groups["name"].Value;
                var typeStr = match.Groups["type"].Value;

                if (!TypeMap.TryGetValue(typeStr, out var type))
                    throw new ArgumentException(
                        $"Unknown type '{typeStr}' in parameter '{name}'",
                        nameof(template));

                var variable = new RouteVariable(name, type);
                variables.Add(variable);
                segments.Add(new DynamicTemplateSegment(variable));
            }
            else
            {
                segments.Add(new StaticTemplateSegment(part));
            }
        }

        return (segments, variables);
    }
}
