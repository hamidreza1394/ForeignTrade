using System.Security.Claims;
using ForeignTrade.Server.Data;
using ForeignTrade.Server.Domain;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ForeignTrade.Server.Api;

public static class WorkContextEndpoints
{
    private const string CompanyClaim = "ft:company_id";
    private const string PeriodClaim = "ft:period_id";

    public static RouteGroupBuilder MapWorkContextEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/context", GetContextAsync).RequireAuthorization();
        api.MapPost("/context", SelectContextAsync).RequireAuthorization();
        api.MapGet("/companies/{companyId:guid}/overview", GetCompanyOverviewAsync).RequireAuthorization();
        return api;
    }


    private static async Task<IResult> GetCompanyOverviewAsync(
        Guid companyId, ClaimsPrincipal principal, UserManager<ApplicationUser> users,
        ForeignTradeDbContext db, CancellationToken ct)
    {
        var user = await users.GetUserAsync(principal);
        if (user is null || !user.IsActive) return Results.Unauthorized();

        var company = await (from membership in db.UserCompanyMemberships.AsNoTracking()
            join item in db.Companies.AsNoTracking() on membership.CompanyId equals item.Id
            join tenant in db.Tenants.AsNoTracking() on membership.TenantId equals tenant.Id
            where membership.UserId == user.Id && membership.IsActive && item.IsActive &&
                  tenant.IsActive && item.TenantId == membership.TenantId && item.Id == companyId
            select new { Company = item, membership.RoleName }).SingleOrDefaultAsync(ct);

        if (company is null) return Results.NotFound(new { title = "شرکت پیدا نشد یا به آن دسترسی ندارید." });

        var periods = await db.OperationalPeriods.AsNoTracking()
            .Where(period => period.CompanyId == companyId && period.TenantId == company.Company.TenantId)
            .OrderByDescending(period => period.StartDate)
            .Select(period => new PeriodSummary(period.Id, period.Title, period.StartDate, period.EndDate, period.Status.ToString()))
            .ToListAsync(ct);

        return Results.Ok(new CompanyOverview(
            new CompanySummary(company.Company.Id, company.Company.Name, company.Company.NationalId, company.RoleName),
            periods));
    }

    private static async Task<IResult> GetContextAsync(
        ClaimsPrincipal principal, UserManager<ApplicationUser> users, ForeignTradeDbContext db, CancellationToken ct)
    {
        var user = await users.GetUserAsync(principal);
        if (user is null || !user.IsActive) return Results.Unauthorized();

        var memberships = await (from membership in db.UserCompanyMemberships.AsNoTracking()
            join company in db.Companies.AsNoTracking() on membership.CompanyId equals company.Id
            join tenant in db.Tenants.AsNoTracking() on membership.TenantId equals tenant.Id
            where membership.UserId == user.Id && membership.IsActive && company.IsActive &&
                  tenant.IsActive && company.TenantId == membership.TenantId
            orderby company.Name
            select new { Company = company, membership.RoleName }).ToListAsync(ct);

        if (memberships.Count == 0)
            return Results.Ok(new WorkContextResponse(null, "noCompany", null, []));

        var companyIds = memberships.Select(x => x.Company.Id).ToArray();
        var periods = await (from period in db.OperationalPeriods.AsNoTracking()
            join company in db.Companies.AsNoTracking() on period.CompanyId equals company.Id
            where companyIds.Contains(company.Id) && period.TenantId == company.TenantId
            orderby period.StartDate descending, period.Title
            select period).ToListAsync(ct);

        var periodMap = periods.GroupBy(x => x.CompanyId).ToDictionary(x => x.Key, x => x.ToArray());
        var companies = memberships.Select(m => new CompanyChoice(
            m.Company.Id, m.Company.Name, m.Company.NationalId, m.RoleName,
            periodMap.TryGetValue(m.Company.Id, out var list)
                ? list.Select(ToChoice).ToArray()
                : [],
            ["independent"])).ToArray();

        var eligible = memberships.SelectMany(m =>
            (periodMap.TryGetValue(m.Company.Id, out var list) ? list : [])
            .Where(p => p.Status is OperationalPeriodStatus.Open or OperationalPeriodStatus.Closed)
            .Select(p => new DirectSelection(m.Company.Id, p.Id))).ToArray();

        var current = await GetCurrentAsync(principal, db, user.Id, ct);
        var entry = current is not null ? "selectionRequired" :
            memberships.Count == 1 && eligible.Length == 1 ? "directEntry" :
            eligible.Length == 0 ? "noEligiblePeriod" : "selectionRequired";
        return Results.Ok(new WorkContextResponse(current, entry,
            memberships.Count == 1 && eligible.Length == 1 ? eligible[0] : null, companies));
    }

    private static async Task<IResult> SelectContextAsync(
        SelectContextRequest request, HttpContext context, IAntiforgery antiforgery,
        UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
        ForeignTradeDbContext db, CancellationToken ct)
    {
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest(new { title = "درخواست انتخاب دوره معتبر نیست." }); }

        var user = await users.GetUserAsync(context.User);
        if (user is null || !user.IsActive) return Results.Unauthorized();

        var selection = await (from membership in db.UserCompanyMemberships
            join company in db.Companies on membership.CompanyId equals company.Id
            join period in db.OperationalPeriods on new { membership.TenantId, CompanyId = company.Id } equals new { period.TenantId, period.CompanyId }
            join tenant in db.Tenants on membership.TenantId equals tenant.Id
            where membership.UserId == user.Id && membership.IsActive && company.IsActive &&
                  company.TenantId == membership.TenantId && tenant.IsActive && period.Id == request.PeriodId &&
                  (period.Status == OperationalPeriodStatus.Open || period.Status == OperationalPeriodStatus.Closed)
            select new { Company = company, Membership = membership, Period = period }).SingleOrDefaultAsync(ct);

        if (selection is null || selection.Company.Id != request.CompanyId)
            return Results.NotFound(new { title = "شرکت یا دوره انتخاب‌شده در دسترس نیست." });

        var principal = await signIn.CreateUserPrincipalAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("ft:session_id", context.User.FindFirstValue("ft:session_id") ?? Guid.NewGuid().ToString("D")));
        identity.AddClaim(new Claim(CompanyClaim, selection.Company.Id.ToString("D")));
        identity.AddClaim(new Claim(PeriodClaim, selection.Period.Id.ToString("D")));
        await context.SignInAsync(IdentityConstants.ApplicationScheme, principal);

        return Results.Ok(ToSelected(selection.Company, selection.Membership.RoleName, selection.Period));
    }

    private static async Task<SelectedContext?> GetCurrentAsync(ClaimsPrincipal principal, ForeignTradeDbContext db, Guid userId, CancellationToken ct)
    {
        if (!Guid.TryParse(principal.FindFirstValue(CompanyClaim), out var companyId) ||
            !Guid.TryParse(principal.FindFirstValue(PeriodClaim), out var periodId)) return null;

        var current = await (from membership in db.UserCompanyMemberships.AsNoTracking()
            join company in db.Companies.AsNoTracking() on membership.CompanyId equals company.Id
            join period in db.OperationalPeriods.AsNoTracking() on new { membership.TenantId, CompanyId = company.Id } equals new { period.TenantId, period.CompanyId }
            join tenant in db.Tenants.AsNoTracking() on membership.TenantId equals tenant.Id
            where membership.UserId == userId && membership.IsActive && company.IsActive && tenant.IsActive &&
                  company.TenantId == membership.TenantId && company.Id == companyId && period.Id == periodId &&
                  (period.Status == OperationalPeriodStatus.Open || period.Status == OperationalPeriodStatus.Closed)
            select new { Company = company, membership.RoleName, Period = period }).SingleOrDefaultAsync(ct);
        return current is null ? null : ToSelected(current.Company, current.RoleName, current.Period);
    }

    private static PeriodChoice ToChoice(OperationalPeriod period) =>
        new(period.Id, period.Title, period.StartDate, period.EndDate, period.Status.ToString(),
            period.Status is OperationalPeriodStatus.Open or OperationalPeriodStatus.Closed,
            period.Status == OperationalPeriodStatus.Open ? "readWrite" : "readOnly");

    private static SelectedContext ToSelected(Company company, string roleName, OperationalPeriod period) =>
        new(new CompanySummary(company.Id, company.Name, company.NationalId, roleName), ToChoice(period));

    public sealed record SelectContextRequest(Guid CompanyId, Guid PeriodId);
    public sealed record PeriodSummary(Guid Id, string Title, DateOnly StartDate, DateOnly EndDate, string Status);
    public sealed record CompanyOverview(CompanySummary Company, IReadOnlyList<PeriodSummary> Periods);
    public sealed record CompanySummary(Guid Id, string Name, string? NationalId, string RoleName);
    public sealed record PeriodChoice(Guid Id, string Title, DateOnly StartDate, DateOnly EndDate, string Status, bool IsSelectable, string? AccessMode);
    public sealed record CompanyChoice(Guid Id, string Name, string? NationalId, string RoleName, IReadOnlyList<PeriodChoice> Periods, IReadOnlyList<string> RelationshipTypes);
    public sealed record DirectSelection(Guid CompanyId, Guid PeriodId);
    public sealed record SelectedContext(CompanySummary Company, PeriodChoice Period);
    public sealed record WorkContextResponse(SelectedContext? Current, string Entry, DirectSelection? DirectSelection, IReadOnlyList<CompanyChoice> Companies);
}
