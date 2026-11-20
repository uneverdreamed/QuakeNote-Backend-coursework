using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace QuakeNote.Earthquakes.Data;

public static class EarthquakesDataExtensions
{
    public static IServiceCollection AddEarthquakesData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<EarthquakesDbContext>(options => options
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(EarthquakesDbContext).Assembly.GetName().Name))
            .UseSnakeCaseNamingConvention());
        return services;
    }
}