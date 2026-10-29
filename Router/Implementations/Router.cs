using Interfaces;

namespace Router.Implementations;

public class Router : IRouter
{
    public void RegisterRoute(string template, Action action)
    {
        throw new NotImplementedException();

        //Спарсить

        //Проверить соответсвие Handler - Template

        //Сохранить
    }

    public void RegisterRoute<T1>(string template, Action<T1> action)
    {
        throw new NotImplementedException();

        //Спарсить

        //Проверить соответсвие Handler - Template

        //Сохранить
    }

    public void RegisterRoute<T1, T2>(string template, Action<T1, T2> action)
    {
        throw new NotImplementedException();

        //Спарсить

        //Проверить соответсвие Handler - Template

        //Сохранить
    }

    public void Route(string route)
    {
        throw new NotImplementedException();

        //Распарсить - Сегментировать маршрут

        //Получить запись RouteRegistration (template + HandlerMetadata)

        //Model Bind - получить dict<name, value> из маршрута по соответсвующему шаблону

        //HabdlerRunner - запустить Action HandlerMetadata на выполнения с подстановкой значений из результата Binding
    }
}
