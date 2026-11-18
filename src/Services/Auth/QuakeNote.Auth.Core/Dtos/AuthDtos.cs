namespace QuakeNote.Auth.Core.Dtos;

public record RequestCodeDto(string Email);

public record RequestCodeResponseDto(DateTimeOffset ExpiresAt);

/// <param name="BirthDate">Обязательна для нового пользователя.</param>
/// <param name="AcceptTerms">Обязательно true для нового пользователя.</param>
public record VerifyCodeDto(string Email, string Code, DateOnly? BirthDate, bool? AcceptTerms);

public record MeDto(Guid Uid, string Email, string Role, DateTimeOffset CreatedAt);

public record VerifyCodeResponseDto(string Token, DateTimeOffset ExpiresAt, MeDto User);