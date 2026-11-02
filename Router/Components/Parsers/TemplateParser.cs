using Router.Exceptions;
using Router.Models.Segments;
using Router.Models.Templates;
using Router.Models.Variables;
using System.Text.RegularExpressions;

namespace Router.Components.Compilers;

internal class TemplateParser
{
    private static readonly Regex SegmentRegex = new(
        @"(?<!\{)\{(?<name>[^:{}]+):(?<type>[^{}]+)\}(?!\})",
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
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Template cannot be null or empty", nameof(template));

        var (segments, variables) = SegmentTemplate(template);
        return new Template(segments, variables);
    }

    private (IEnumerable<TemplateSegment>, IEnumerable<RouteVariable>) SegmentTemplate(string template)
    {
        var segments = new List<TemplateSegment>();
        var variables = new List<RouteVariable>();
        var variableNames = new HashSet<string>();

        var parts = template.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var match = SegmentRegex.Match(part);

            if (match.Success)
            {
                var name = match.Groups["name"].Value;
                var typeStr = match.Groups["type"].Value;
               
                if (variableNames.Contains(name))
                    throw new InvalidTemplateFormatException(
                        $"Duplicate parameter name '{name}' found in template",
                        template);

                if (!TypeMap.TryGetValue(typeStr, out var type))
                    throw new InvalidTemplateFormatException(
                        $"Unknown type '{typeStr}' in parameter '{name}'",
                        template);

                variableNames.Add(name);
                var variable = new RouteVariable(name, type);
                variables.Add(variable);
                segments.Add(new DynamicTemplateSegment(variable));
            }
            else
            {
                if (part.Contains('{') || part.Contains('}'))
                    throw new InvalidTemplateFormatException(
                        $"Invalid parameter format in segment '{part}'. Expected format: {{name:type}}",
                        template);

                segments.Add(new StaticTemplateSegment(part));
            }
        }

        return (segments, variables);
    }
}
