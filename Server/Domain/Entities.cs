using Microsoft.AspNetCore.Identity;

namespace ForeignTrade.Server.Domain;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class Company
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class UserCompanyMembership
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public string RoleName { get; set; } = "کاربر";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
}

public enum OperationalPeriodStatus
{
    Open = 1,
    Closed = 2,
    Archived = 3,
}

public sealed class OperationalPeriod
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public OperationalPeriodStatus Status { get; set; } = OperationalPeriodStatus.Open;
    public DateTime CreatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
