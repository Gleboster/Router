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

            if (match.Success &&
                match.Length == part.Length)
            {
                var name = match.Groups["name"].Value;
                var typeStr = match.Groups["type"].Value;

                EnsureUniqueVariableName(template, variableNames, name);

                Type type = GetSupportedType(template, name, typeStr);

                variableNames.Add(name);
                var variable = new RouteVariable(name, type);
                variables.Add(variable);
                segments.Add(new DynamicTemplateSegment(variable));
            }
            else
            {
                EnsureValidStaticSegment(template, part);

                segments.Add(new StaticTemplateSegment(part));
            }
        }

        return (segments, variables);
    }

    private static void EnsureValidStaticSegment(string template, string part)
    {
        if (part.Contains('{') ||
            part.Contains('}') ||
            part.Contains(':'))
            throw new InvalidTemplateFormatException(
                $"Invalid parameter format in segment '{part}'. Expected format: {{name:type}}",
                template);

        if (string.IsNullOrEmpty(part) ||
            !part.All(c => char.IsAsciiLetter(c) || char.IsAsciiDigit(c)))
            throw new InvalidTemplateFormatException(
                $"Invalid static segment '{part}'. Only Latin letters and digits are allowed.",
                template);
    }

    private static Type GetSupportedType(string template, string name, string typeStr)
    {
        if (!TypeMap.TryGetValue(typeStr, out var type))
            throw new InvalidTemplateFormatException(
                $"Unknown type '{typeStr}' in parameter '{name}'",
                template);
        
        return type;
    }

    private static void EnsureUniqueVariableName(string template, HashSet<string> variableNames, string name)
    {
        if (variableNames.Contains(name))
            throw new InvalidTemplateFormatException(
                $"Duplicate parameter name '{name}' found in template",
                template);
    }
}
