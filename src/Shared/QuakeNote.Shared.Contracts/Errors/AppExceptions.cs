namespace QuakeNote.Shared.Contracts.Errors;

public abstract class AppException(string code, string message, int statusCode) : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}

public class BadRequestException(string message, string code = "bad_request")
    : AppException(code, message, 400);

public class UnauthorizedException(string message, string code = "unauthorized")
    : AppException(code, message, 401);

public class ForbiddenException(string message, string code = "forbidden")
    : AppException(code, message, 403);

public class NotFoundException(string message, string code = "not_found")
    : AppException(code, message, 404);

public class ConflictException(string message, string code = "conflict")
    : AppException(code, message, 409);

public class GoneException(string message, string code = "gone")
    : AppException(code, message, 410);

public class TooManyRequestsException(string message, string code = "too_many_requests")
    : AppException(code, message, 429);

public class ValidationFailedException(IReadOnlyDictionary<string, string[]> errors)
    : AppException("validation_failed", "Ошибка валидации входных данных", 400)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}