using Router.DTOs.ModelBinding;
using Router.Models.Routes;
using Router.Models.Segments;
using Router.Models.Templates;
using Router.Utils;

namespace Router.Components.Binding;

public class ModelBinder
{
    public ModelBindResult ModelBind(Template template, Route route)
    {
        var boundObjects = new List<IModelBindObject>();

        int routeIndex = 0;

        foreach (var templateSegment in template.Segments)
        {
            if (routeIndex >= route.Segments.Count)
                throw new ArgumentException(
                    $"Route has fewer segments than template requires. Expected at least {routeIndex + 1}, got {route.Segments.Count}.");

            var routeSegment = route.Segments[routeIndex];

            if (templateSegment is DynamicTemplateSegment dynamicSegment)
            {
                var variable = dynamicSegment.RouteVariable;

                if (!routeSegment.TryParseValue(variable.Type, out var parsedValue))
                    throw new ArgumentException(
                        $"Cannot convert value '{routeSegment.Value}' to type {variable.Type.Name} for parameter '{variable.Name}'.");

                var boundObject = CreateModelBindObject(variable.Name, variable.Type, parsedValue);
                boundObjects.Add(boundObject);
            }

            routeIndex++;
        }

        return new ModelBindResult(boundObjects);
    }

    private static IModelBindObject CreateModelBindObject(string name, Type type, object value)
    {
        var modelBindObjectType = typeof(ModelBindObject<>).MakeGenericType(type);
        return (IModelBindObject)Activator.CreateInstance(modelBindObjectType, name, value)!;
    }
}