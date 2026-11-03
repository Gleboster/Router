namespace Router.Exceptions;

public class InvalidModelBindResultException : InvalidOperationException
{
    public InvalidModelBindResultException(string message)
        : base(message)
    {
    }
}
