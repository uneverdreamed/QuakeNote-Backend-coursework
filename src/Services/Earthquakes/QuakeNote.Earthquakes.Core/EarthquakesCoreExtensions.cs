using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuakeNote.Earthquakes.Core.Abstractions;
using QuakeNote.Earthquakes.Core.Infrastructure;
using QuakeNote.Earthquakes.Core.Services;
using QuakeNote.Earthquakes.Core.Validators;

namespace QuakeNote.Earthquakes.Core;

public static class EarthquakesCoreExtensions
{
    /// <remarks>gRPC-клиент Reports (ReportsGrpc.ReportsGrpcClient) регистрируется в API.</remarks>
    public static IServiceCollection AddEarthquakesCore(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<EarthquakeCache>();
        services.AddScoped<IReportsGrpcClient, ReportsGrpcClient>();

        services.AddScoped<EarthquakeService>();
        services.AddScoped<StationService>();
        services.AddScoped<StationReadingService>();
        services.AddScoped<EventAssessmentService>();

        services.AddValidatorsFromAssemblyContaining<CreateEarthquakeDtoValidator>();
        return services;
    }
}