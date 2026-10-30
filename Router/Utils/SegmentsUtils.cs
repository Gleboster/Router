using Router.Models.Segments;
using System.Collections.Generic;
using System.Globalization;

namespace Router.Utils;

public static class SegmentsUtils
{
    private static StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;
    
    private static CultureInfo cultureInfo = CultureInfo.InvariantCulture;

    private static DateTimeStyles dateTimeStyle = DateTimeStyles.RoundtripKind;

    public static bool MatchTemplateSegment(
        this RouteSegment routeSegment,
        TemplateSegment templateSegment)
    {
        return templateSegment switch
        {
            StaticTemplateSegment staticSegment =>
                string.Equals(staticSegment.Value, routeSegment.Value, StringComparison.OrdinalIgnoreCase),
            DynamicTemplateSegment dynamicSegment => 
                routeSegment.TryParseValue(dynamicSegment.RouteVariable.Type, out _),
            _ => false
        };
    }

    public static bool Equal(
        this TemplateSegment main, 
        TemplateSegment other)
    {
        return (main, other) switch
        {
            (StaticTemplateSegment s1, StaticTemplateSegment s2) =>
                string.Equals(s1.Value, s2.Value, stringComparison),
            (DynamicTemplateSegment d1, DynamicTemplateSegment d2) =>
                d1.RouteVariable.Name == d2.RouteVariable.Name &&
                d1.RouteVariable.Type == d2.RouteVariable.Type,
            _ => false
        };
    }

    public static bool TryParseValue(
        this RouteSegment routeSegment,
        Type type,
        out object result)
    {
        result = default!;
        var value = routeSegment?.Value;
        if (value is null)
            return false;

        object? parsedValue;

        if (type == typeof(string))
            parsedValue = value;

        else if (type == typeof(int))
        {
            if (!int.TryParse(value, NumberStyles.Integer, cultureInfo, out var parsed))
                return false;

            parsedValue = parsed;
        }
        else if (type == typeof(Guid))
        {
            if (!Guid.TryParse(value, out var parsed))
                return false;

            parsedValue = parsed;
        }
        else if (type == typeof(float))
        {
            if (!float.TryParse(value, NumberStyles.Float, cultureInfo, out var parsed))
                return false;

            parsedValue = parsed;
        }
        else if (type == typeof(DateTime))
        {
            if (!DateTime.TryParse(value, cultureInfo, dateTimeStyle, out var parsed))
                return false;

            parsedValue = parsed;
        }
        else
            return false;

        if (!type.IsInstanceOfType(parsedValue))
            return false;

        result = parsedValue;
        return true;
    }
}