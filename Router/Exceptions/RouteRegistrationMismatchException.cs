namespace Router.Exceptions;

public class RouteRegistrationMismatchException : ArgumentException
{
    public RouteRegistrationMismatchException(string message)
        : base(message)
    {
    }
}
