namespace QuakeNote.Shared.Web.Auth;

public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string ServiceToService = "ServiceToService";
}

public static class ServiceAuthConstants
{
    public const string HeaderName = "X-Service-Secret";
}

public static class JwtDefaults
{
    public const string Issuer = "quakenote-auth";
    public const string Audience = "quakenote-api";
}