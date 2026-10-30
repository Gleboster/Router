namespace Router.DTOs.ModelBinding;

public interface IModelBindObject
{
    public string Name { get; }

    public Type Type { get; }
}

public class ModelBindObject<T>(string name, T value) 
    : IModelBindObject
{
    public string Name { get; } = name;

    public Type Type { get; } = typeof(T);

    public T Value { get; } = value; //TODO Value дублируется есть obj-и дженерик
}