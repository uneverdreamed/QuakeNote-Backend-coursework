using System.Security.Claims;
using QuakeNote.Shared.Contracts;

namespace QuakeNote.Shared.Web.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserUid(this ClaimsPrincipal user)
        => Guid.TryParse(user.FindFirst("sub")?.Value, out var uid) ? uid : null;

    public static string? GetJti(this ClaimsPrincipal user) => user.FindFirst("jti")?.Value;

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.Admin);

    public static DateTimeOffset? GetExpiresAt(this ClaimsPrincipal user)
    => long.TryParse(user.FindFirst("exp")?.Value, out var exp)
        ? DateTimeOffset.FromUnixTimeSeconds(exp)
        : null;
}