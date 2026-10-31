using Xunit;
using ForeignTrade.Server.Domain;

namespace ForeignTrade.Server.Tests.Domain;

public sealed class OperationalPeriodPolicyTests
{
    [Theory]
    [InlineData(OperationalPeriodStatus.Open, true, "readWrite")]
    [InlineData(OperationalPeriodStatus.Closed, true, "readOnly")]
    [InlineData(OperationalPeriodStatus.Archived, false, null)]
    public void Selection_policy_matches_period_status(
        OperationalPeriodStatus status, bool expectedSelectable, string? expectedAccessMode)
    {
        Assert.Equal(expectedSelectable, OperationalPeriodPolicy.IsSelectable(status));
        Assert.Equal(expectedAccessMode, OperationalPeriodPolicy.AccessMode(status));
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(2, 1, false)]
    [InlineData(1, 2, false)]
    [InlineData(0, 0, false)]
    public void Direct_entry_requires_exactly_one_company_and_one_period(
        int activeCompanyCount, int selectablePeriodCount, bool expected)
    {
        Assert.Equal(expected, OperationalPeriodPolicy.CanEnterDirectly(activeCompanyCount, selectablePeriodCount));
    }

    [Fact]
    public void Period_date_range_must_not_be_reversed()
    {
        Assert.True(OperationalPeriodPolicy.HasValidDateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
        Assert.False(OperationalPeriodPolicy.HasValidDateRange(new DateOnly(2027, 1, 1), new DateOnly(2026, 12, 31)));
    }
}
