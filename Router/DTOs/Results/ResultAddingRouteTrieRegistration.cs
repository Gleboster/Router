namespace Router.DTOs.Results;

internal enum AddRouteStatus
{
    Success,
    Duplicate
}

internal record ResultAddingRouteTrieRegistration(AddRouteStatus Status)
{
    public static ResultAddingRouteTrieRegistration Success()
        => new(AddRouteStatus.Success);

    public static ResultAddingRouteTrieRegistration Duplicate()
        => new(AddRouteStatus.Duplicate);
}