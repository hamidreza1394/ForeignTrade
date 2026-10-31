namespace ForeignTrade.Server.Domain;

public static class OperationalPeriodPolicy
{
    public static bool IsSelectable(OperationalPeriodStatus status) =>
        status is OperationalPeriodStatus.Open or OperationalPeriodStatus.Closed;

    public static string? AccessMode(OperationalPeriodStatus status) => status switch
    {
        OperationalPeriodStatus.Open => "readWrite",
        OperationalPeriodStatus.Closed => "readOnly",
        _ => null,
    };

    public static bool CanEnterDirectly(int activeCompanyCount, int selectablePeriodCount) =>
        activeCompanyCount == 1 && selectablePeriodCount == 1;

    public static bool HasValidDateRange(DateOnly startDate, DateOnly endDate) => startDate <= endDate;
}
