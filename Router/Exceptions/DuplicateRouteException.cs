namespace Router.Exceptions;

public class DuplicateRouteException : InvalidOperationException
{
    public string Template { get; }

    public DuplicateRouteException(string template, string message)
        : base(message)
    {
        Template = template;
    }
}
