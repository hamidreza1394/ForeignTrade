using System.Security.Claims;
using ForeignTrade.Server.Domain;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace ForeignTrade.Server.Api;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/antiforgery/token", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { headerName = antiforgery.Options.HeaderName, requestToken = tokens.RequestToken });
        }).AllowAnonymous();

        api.MapPost("/auth/login", LoginAsync).AllowAnonymous().RequireRateLimiting("login");
        api.MapGet("/session", GetSessionAsync).RequireAuthorization();
        api.MapPost("/session/logout", LogoutAsync).RequireAuthorization();
        return api;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        HttpContext context,
        IAntiforgery antiforgery,
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        CancellationToken cancellationToken)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest(new { title = "درخواست ورود معتبر نیست. صفحه را تازه‌سازی کنید." });
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            return Results.BadRequest(new { title = "نام کاربری و گذرواژه را وارد کنید." });

        var user = await users.FindByNameAsync(request.Username.Trim());
        if (user is null || !user.IsActive)
            return Results.Unauthorized();

        var result = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Results.Problem(title: "تعداد تلاش‌های ورود بیش از حد مجاز است. کمی بعد دوباره تلاش کنید.", statusCode: StatusCodes.Status429TooManyRequests);
            return Results.Unauthorized();
        }

        var principal = await signIn.CreateUserPrincipalAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("ft:session_id", Guid.NewGuid().ToString("D")));
        await context.SignInAsync(IdentityConstants.ApplicationScheme, principal);
        return Results.Ok(new SessionResponse(user.UserName ?? string.Empty, user.FullName, "User", false));
    }

    private static async Task<IResult> GetSessionAsync(UserManager<ApplicationUser> users, ClaimsPrincipal principal)
    {
        var user = await users.GetUserAsync(principal);
        if (user is null || !user.IsActive) return Results.Unauthorized();
        return Results.Ok(new SessionResponse(user.UserName ?? string.Empty, user.FullName, "User", await users.IsInRoleAsync(user, "Administrator")));
    }

    private static async Task<IResult> LogoutAsync(HttpContext context, IAntiforgery antiforgery, SignInManager<ApplicationUser> signIn)
    {
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest(new { title = "درخواست خروج معتبر نیست." }); }
        await signIn.SignOutAsync();
        return Results.NoContent();
    }

    public sealed record LoginRequest(string Username, string Password);
    public sealed record SessionResponse(string Username, string FullName, string AccountKind, bool IsAdministrator);
}
