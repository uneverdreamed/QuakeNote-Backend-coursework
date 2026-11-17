using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace QuakeNote.Shared.Web.Auth;

public sealed class ServiceAuthOptions
{
    public string Secret { get; set; } = string.Empty;
}

public sealed class ServiceSecretRequirement : IAuthorizationRequirement;

public static class ServiceSecretComparer
{
    public static bool AreEqual(string? provided, string expected)
    {
        if (string.IsNullOrEmpty(provided) || string.IsNullOrEmpty(expected)) return false;

        var a = Encoding.UTF8.GetBytes(provided);
        var b = Encoding.UTF8.GetBytes(expected);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }
}

public sealed class ServiceSecretAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptions<ServiceAuthOptions> options) : AuthorizationHandler<ServiceSecretRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ServiceSecretRequirement requirement)
    {
        var request = httpContextAccessor.HttpContext?.Request;

        if (request is not null &&
            request.Headers.TryGetValue(ServiceAuthConstants.HeaderName, out var provided) &&
            ServiceSecretComparer.AreEqual(provided.ToString(), options.Value.Secret))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}