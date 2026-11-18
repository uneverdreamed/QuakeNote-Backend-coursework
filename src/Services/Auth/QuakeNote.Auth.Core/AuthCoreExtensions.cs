using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuakeNote.Auth.Core.Abstractions;
using QuakeNote.Auth.Core.Infrastructure;
using QuakeNote.Auth.Core.Options;
using QuakeNote.Auth.Core.Services;
using QuakeNote.Auth.Core.Validators;

namespace QuakeNote.Auth.Core;

public static class AuthCoreExtensions
{
    public static IServiceCollection AddAuthCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<AuthOptions>(configuration.GetSection("Auth"));

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<ICodeHasher, BCryptCodeHasher>();
        services.AddSingleton<IJwtService, JwtService>();

        services.AddScoped<AuthService>();
        services.AddScoped<ProfileService>();
        services.AddScoped<SettingsService>();

        services.AddValidatorsFromAssemblyContaining<RequestCodeDtoValidator>();
        return services;
    }
}