using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace QuakeNote.Auth.Data;

public static class AuthDataExtensions
{
    public static IServiceCollection AddAuthData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuthDbContext>(options => options
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AuthDbContext).Assembly.GetName().Name))
            .UseSnakeCaseNamingConvention());

        return services;
    }
}