namespace Router.Exceptions;

public class ModelBindingException : ArgumentException
{
    public ModelBindingException(string message)
        : base(message)
    {
    }
}
