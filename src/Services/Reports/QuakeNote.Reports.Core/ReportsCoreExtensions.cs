using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuakeNote.Reports.Core.Abstractions;
using QuakeNote.Reports.Core.Infrastructure;
using QuakeNote.Reports.Core.Services;
using QuakeNote.Reports.Core.Validators;

namespace QuakeNote.Reports.Core;

public static class ReportsCoreExtensions
{
    public static IServiceCollection AddReportsCore(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IEditWindowPolicy, FifteenMinuteEditWindow>();

        services.AddScoped<ReportStatsService>();
        services.AddScoped<ReportService>();
        services.AddScoped<DetailedReportService>();

        services.AddValidatorsFromAssemblyContaining<CreateReportDtoValidator>();
        return services;
    }
}