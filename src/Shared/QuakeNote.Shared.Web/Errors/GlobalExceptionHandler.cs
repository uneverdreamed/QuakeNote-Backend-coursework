using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuakeNote.Shared.Contracts.Errors;

namespace QuakeNote.Shared.Web.Errors;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int Status, string Code, string Message, IReadOnlyDictionary<string, string[]>? Errors) result =
            exception switch
            {
                ValidationFailedException v => (v.StatusCode, v.Code, v.Message, v.Errors),
                AppException a => (a.StatusCode, a.Code, a.Message, null),
                BadHttpRequestException => (StatusCodes.Status400BadRequest, "bad_request", "Некорректный запрос", null),
                _ => (StatusCodes.Status500InternalServerError, "internal_error", "Внутренняя ошибка сервера", null)
            };

        if (result.Status >= 500)
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);
        else
            logger.LogInformation("Request failed with {Code}: {Message}", result.Code, result.Message);

        httpContext.Response.StatusCode = result.Status;

        var problem = new ProblemDetails { Status = result.Status, Title = result.Message };
        problem.Extensions["code"] = result.Code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        if (result.Errors is not null)
            problem.Extensions["errors"] = result.Errors;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}