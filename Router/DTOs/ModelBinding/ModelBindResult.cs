using Router.Models.Segments;
using Router.Models.Variables;
using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Router.DTOs.ModelBinding;

internal class ModelBindResult
{
    private readonly FrozenDictionary<string, ModelBindObject> valuesByName;

    public ModelBindResult(
        IEnumerable<ModelBindObject> variables)
    {
        valuesByName = variables.ToFrozenDictionary(
            variable => variable.Name);
    }

    public ModelBindObject GetValueByName(string name)
    {
        valuesByName.TryGetValue(name, out var value);

        return value;
    }

}