using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using QuakeNote.Shared.Web.Errors;

namespace QuakeNote.Shared.Web.Extensions;

public static class ErrorHandlingExtensions
{
    public static IMvcBuilder AddQuakeNoteControllers(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services
            .AddControllers(options => options.Filters.Add<FluentValidationFilter>())
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value is { Errors.Count: > 0 })
                        .ToDictionary(
                            e => e.Key,
                            e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

                    var problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Ошибка валидации входных данных"
                    };
                    problem.Extensions["code"] = "validation_failed";
                    problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    problem.Extensions["errors"] = errors;

                    return new ObjectResult(problem)
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });
    }

    public static IApplicationBuilder UseQuakeNoteErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages(); // 401/403/404 без тела тоже получат единый формат
        return app;
    }
}