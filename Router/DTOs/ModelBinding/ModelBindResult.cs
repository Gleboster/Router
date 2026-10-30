using Router.Models.Segments;
using Router.Models.Variables;
using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Router.DTOs.ModelBinding;

public class ModelBindResult
{
    private readonly FrozenDictionary<string, IModelBindObject> valuesByName;

    public ModelBindResult(
        IEnumerable<IModelBindObject> variables)
    {
        valuesByName = variables.ToFrozenDictionary(
            variable => variable.Name);
    }

    public T GetValueByName<T>(string name)
    {
        if (!valuesByName.TryGetValue(name, out var value))
            throw new KeyNotFoundException(name);

        if (value is not ModelBindObject<T> typedValue)
            throw new InvalidOperationException(
                $"Expected {typeof(T)}, got {value.Type}");

        return typedValue.Value;
    }
}