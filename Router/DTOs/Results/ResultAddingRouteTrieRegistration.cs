namespace Router.DTOs.Results;

public enum AddRouteStatus
{
    Success,
    Duplicate
}

public record ResultAddingRouteTrieRegistration(AddRouteStatus Status)
{
    public static ResultAddingRouteTrieRegistration Success()
        => new(AddRouteStatus.Success);

    public static ResultAddingRouteTrieRegistration Duplicate()
        => new(AddRouteStatus.Duplicate);
}