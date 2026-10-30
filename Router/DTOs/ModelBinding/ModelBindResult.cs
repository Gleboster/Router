using Router.Models.Segments;
using Router.Models.Variables;
using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Router.DTOs.ModelBinding;

public class ModelBindResult
{
    private readonly FrozenDictionary<string, ModelBindObject> valuesByName;

    public ModelBindResult(
        IEnumerable<ModelBindObject> variables)
    {
        valuesByName = variables.ToFrozenDictionary(
            variable => variable.Name);
    }
    public object GetValueByNameWithType(string name, Type type)
    {
        if (!valuesByName.TryGetValue(name, out var value))
            throw new KeyNotFoundException(name);

        if (value.Type != type)
            throw new InvalidOperationException(
                $"Expected {type}, got {value.Type}");

        return value.Value;
    }

}