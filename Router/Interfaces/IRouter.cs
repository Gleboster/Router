namespace Interfaces;

public interface IRouter
{
    /// <summary>
    /// регистрирует маршрут и обработчик без параметров
    /// </summary>
    /// <param name="template">шаблон</param>
    /// <param name="action">обработчик</param>
    void RegisterRoute(string template, Action action);

    /// <summary>
    /// регистрирует маршрут
    /// </summary>
    /// <typeparam name="T1">тип переменной</typeparam>
    /// <param name="template">шаблон</param>
    /// <param name="action">обработчик</param>
    void RegisterRoute<T1>(string template, Action<T1> action);

    /// <summary>
    /// регистрирует маршрут
    /// </summary>
    /// <typeparam name="T1">тип первой переменной</typeparam>
    /// <typeparam name="T2">тип второй переменной</typeparam>
    /// <param name="template">шаблон</param>
    /// <param name="action">обработчик</param>
    void RegisterRoute<T1, T2>(string template, Action<T1, T2> action);

    /// <summary>
    /// вызывает обработчик соответствующий маршруту
    /// </summary>
    /// <param name="route">маршрут</param>
    void Route(string route);
}