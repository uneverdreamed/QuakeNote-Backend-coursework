using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace QuakeNote.Reports.Data;

public static class ReportsDataExtensions
{
    public static IServiceCollection AddReportsData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ReportsDbContext>(options => options
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ReportsDbContext).Assembly.GetName().Name))
            .UseSnakeCaseNamingConvention());
        return services;
    }
}