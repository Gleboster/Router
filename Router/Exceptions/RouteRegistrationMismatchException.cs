namespace Router.Exceptions;

public class RouteRegistrationMismatchException : InvalidOperationException
{
    public RouteRegistrationMismatchException(string message)
        : base(message)
    {
    }
}
