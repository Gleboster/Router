namespace Router.Exceptions;

public class InvalidTemplateFormatException : ArgumentException
{
    public InvalidTemplateFormatException(string message, string template)
        : base(message, nameof(template))
    {
    }
}
