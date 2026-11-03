namespace Router.Exceptions;

public class HandlerExecutionException : InvalidOperationException
{
    public HandlerExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
