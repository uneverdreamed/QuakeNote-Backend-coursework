using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

namespace QuakeNote.Shared.Web.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddQuakeNoteLogging(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .WriteTo.Console());

        return builder;
    }
}