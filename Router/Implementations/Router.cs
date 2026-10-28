using Interfaces;

namespace Router.Implementations;

public class Router : IRouter
{
    public void RegisterRoute<T1>(string template, Action<T1> action)
    {
        throw new NotImplementedException();
    }

    public void RegisterRoute<T1, T2>(string template, Action<T1, T2> action)
    {
        throw new NotImplementedException();
    }

    public void Route(string route)
    {
        throw new NotImplementedException();
    }
}
