using Router.DTOs;
using Router.DTOs.ModelBinding;
using Router.Exceptions;
using Router.Models.Routes;
using Router.Models.Segments;
using Router.Models.Templates;
using Router.Models.Variables;
using Router.Utils;

namespace Router.Components.Binding;

internal class ModelBinder
{
    public ModelBindResult ModelBind(Template template, Route route)
    {
        var boundObjects = new List<ModelBindObject>();

        int routeIndex = 0;

        foreach (var templateSegment in template.Segments)
        {
            var routeSegment = route.Segments[routeIndex];

            if (templateSegment is DynamicTemplateSegment dynamicSegment)
            {
                var variable = dynamicSegment.RouteVariable;

                object parsedValue = GetParsedObject(routeSegment, variable);

                var boundObject = new ModelBindObject(variable.Name, variable.Type, parsedValue);
                boundObjects.Add(boundObject);
            }

            routeIndex++;
        }

        return new ModelBindResult(boundObjects);
    }

    private static object GetParsedObject(
        RouteSegment routeSegment,
        RouteVariable variable)
    {
        if (!routeSegment.TryParseValue(variable.Type, out var parsedValue))
            throw new ModelBindingException(
                $"Cannot convert value '{routeSegment.Value}' to type {variable.Type.Name}" +
                $" for parameter '{variable.Name}'.");
        return parsedValue;
    }
}