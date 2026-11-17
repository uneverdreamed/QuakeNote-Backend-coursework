using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuakeNote.Shared.Contracts;
using QuakeNote.Shared.Web.Auth;

namespace QuakeNote.Shared.Web.Extensions;

public static class AuthExtensions
{
    /// <summary>JWT (RS256) по публичному ключу + политика AdminOnly.</summary>
    public static IServiceCollection AddQuakeNoteJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Jwt");
        var issuer = section["Issuer"] ?? JwtDefaults.Issuer;
        var audience = section["Audience"] ?? JwtDefaults.Audience;
        var signingKey = LoadPublicKey(section);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false; // имена claim как в токене: sub, role, jti
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.AdminOnly, policy => policy.RequireRole(Roles.Admin));

        return services;
    }

    /// <summary>Защита internal-эндпоинтов и gRPC общим секретом + политика ServiceToService.</summary>
    public static IServiceCollection AddQuakeNoteServiceAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServiceAuthOptions>()
            .Bind(configuration.GetSection("ServiceAuth"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Secret), "ServiceAuth:Secret is not configured.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationHandler, ServiceSecretAuthorizationHandler>();
        services.AddTransient<ServiceSecretHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.ServiceToService, policy => policy.AddRequirements(new ServiceSecretRequirement()));

        return services;
    }

    private static RsaSecurityKey LoadPublicKey(IConfigurationSection section)
    {
        var pem = section["PublicKey"];

        if (string.IsNullOrWhiteSpace(pem))
        {
            var path = section["PublicKeyPath"];
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                pem = File.ReadAllText(path);
        }

        if (string.IsNullOrWhiteSpace(pem))
            throw new InvalidOperationException("Jwt:PublicKey or Jwt:PublicKeyPath must be configured.");

        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.Replace("\\n", "\n"));
        return new RsaSecurityKey(rsa);
    }
}