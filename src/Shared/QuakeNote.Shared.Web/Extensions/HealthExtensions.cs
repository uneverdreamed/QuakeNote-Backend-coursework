using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QuakeNote.Shared.Web.Extensions;

public static class HealthExtensions
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static IEndpointConventionBuilder MapQuakeNoteHealth(this IEndpointRouteBuilder endpoints)
        => endpoints
            .MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteResponse })
            .AllowAnonymous();

    private static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString().ToLowerInvariant(),
            checks = report.Entries.ToDictionary(
                e => e.Key,
                e => e.Value.Status.ToString().ToLowerInvariant())
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, Json));
    }
}