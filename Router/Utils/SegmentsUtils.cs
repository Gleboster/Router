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
                routeSegment.TryParseValue(dynamicSegment.RouteVariable.Type),
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

    private static bool TryParseValue(
        this RouteSegment routeSegment, 
        Type type)
    {
        if (routeSegment?.Value is null)
            return false;

        var value = routeSegment.Value;

        if (type == typeof(string))
            return true; 

        if (type == typeof(int))
            return int.TryParse(value, NumberStyles.Integer, cultureInfo, out _);

        if (type == typeof(Guid))
            return Guid.TryParse(value, out _);

        if (type == typeof(float))
            return float.TryParse(value, NumberStyles.Float, cultureInfo, out _);

        if (type == typeof(DateTime))
            return DateTime.TryParse(
                value,
                cultureInfo,
                dateTimeStyle,
                out _);

        return false;
    }

}