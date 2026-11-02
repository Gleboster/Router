namespace Router.Exceptions;

public class InvalidHandlerSignatureException : ArgumentException
{
    public InvalidHandlerSignatureException(string message)
        : base(message, "action")
    {
    }
}
