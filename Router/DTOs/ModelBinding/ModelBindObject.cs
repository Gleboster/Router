namespace Router.DTOs;

public class ModelBindObject(string name, Type type, object value) 
{
    public string Name { get; } = name;

    public Type Type { get; } = type;

    public object Value { get; } = value;
}