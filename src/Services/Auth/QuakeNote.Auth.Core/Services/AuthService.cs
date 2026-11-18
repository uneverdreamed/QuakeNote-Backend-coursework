using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using QuakeNote.Auth.Core.Abstractions;
using QuakeNote.Auth.Core.Dtos;
using QuakeNote.Auth.Core.Options;
using QuakeNote.Auth.Data;
using QuakeNote.Auth.Data.Entities;
using QuakeNote.Shared.Cache;
using QuakeNote.Shared.Contracts;
using QuakeNote.Shared.Contracts.Errors;
using QuakeNote.Shared.Contracts.Events;
using QuakeNote.Shared.Messaging;

namespace QuakeNote.Auth.Core.Services;

public class AuthService(
    AuthDbContext db,
    ICodeHasher codeHasher,
    IJwtService jwtService,
    IMessagePublisher publisher,
    ICacheService cache,
    IOptions<AuthOptions> authOptions,
    TimeProvider timeProvider,
    ILogger<AuthService> logger)
{
    private readonly AuthOptions _options = authOptions.Value;

    public async Task<RequestCodeResponseDto> RequestCodeAsync(RequestCodeDto dto, CancellationToken ct = default)
    {
        var email = NormalizeEmail(dto.Email);
        var now = timeProvider.GetUtcNow();

        var cooldownBorder = now.AddSeconds(-_options.RequestCooldownSeconds);
        var tooSoon = await db.AuthCodes.AnyAsync(c => c.Email == email && c.CreatedAt > cooldownBorder, ct);
        if (tooSoon)
            throw new TooManyRequestsException("Код уже был отправлен. Повторите запрос позже.", "code_request_too_frequent");

        // Ранее выданные неиспользованные коды перестают действовать
        await db.AuthCodes
            .Where(c => c.Email == email && c.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.UsedAt, (DateTimeOffset?)now), ct);

        var code = GenerateCode();
        var authCode = new AuthCode
        {
            Email = email,
            CodeHash = codeHasher.Hash(code),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(_options.CodeExpiryMinutes)
        };
        db.AuthCodes.Add(authCode);
        await db.SaveChangesAsync(ct);

        try
        {
            await publisher.PublishAsync(new AuthCodeRequestedEvent(email, code, authCode.ExpiresAt), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish auth.code_requested, rolling back the code");
            db.AuthCodes.Remove(authCode);
            await db.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        return new RequestCodeResponseDto(authCode.ExpiresAt);
    }

    public async Task<VerifyCodeResponseDto> VerifyCodeAsync(VerifyCodeDto dto, CancellationToken ct = default)
    {
        var email = NormalizeEmail(dto.Email);
        var now = timeProvider.GetUtcNow();

        var authCode = await db.AuthCodes
            .Where(c => c.Email == email && c.UsedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (authCode is null)
            throw new UnauthorizedException("Неверный код подтверждения.", "invalid_code");

        if (authCode.ExpiresAt <= now)
            throw new GoneException("Срок действия кода истёк. Запросите новый код.", "code_expired");

        if (authCode.Attempts >= _options.MaxAttempts)
        {
            authCode.UsedAt = now;
            await db.SaveChangesAsync(ct);
            throw new TooManyRequestsException("Превышено число попыток ввода. Запросите новый код.", "too_many_attempts");
        }

        if (!codeHasher.Verify(dto.Code, authCode.CodeHash))
        {
            authCode.Attempts++;
            await db.SaveChangesAsync(ct);
            throw new UnauthorizedException("Неверный код подтверждения.", "invalid_code");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null)
        {
            if (dto.BirthDate is null || dto.AcceptTerms != true)
                throw new BadRequestException(
                    "Для регистрации укажите дату рождения и примите условия использования.",
                    "registration_data_required");

            user = new User
            {
                Email = email,
                BirthDate = dto.BirthDate.Value,
                TermsAcceptedAt = now,
                Profile = new UserProfile(),
                Settings = new UserSettings()
            };
            db.Users.Add(user);
        }

        if (_options.AdminEmails.Contains(user.Email, StringComparer.OrdinalIgnoreCase))
            user.Role = Roles.Admin;

        user.LastLogin = now;
        authCode.UsedAt = now;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException("Не удалось завершить вход. Повторите попытку.", "concurrent_request");
        }

        var token = jwtService.CreateToken(user);
        return new VerifyCodeResponseDto(token.Token, token.ExpiresAt, user.ToMeDto());
    }

    /// <summary>Отзыв токена: его jti попадает в чёрный список Redis до конца срока жизни токена.</summary>
    public async Task LogoutAsync(string jti, DateTimeOffset tokenExpiresAt)
    {
        var ttl = tokenExpiresAt - timeProvider.GetUtcNow();
        if (ttl > TimeSpan.Zero)
            await cache.SetAsync(CacheKeys.AuthBlacklist(jti), "1", ttl);
    }

    public async Task<MeDto> GetMeAsync(Guid userUid, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Uid == userUid, ct)
                   ?? throw new NotFoundException("Пользователь не найден.", "user_not_found");
        return user.ToMeDto();
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string GenerateCode()
        => RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, AuthOptions.CodeLength))
            .ToString($"D{AuthOptions.CodeLength}");
}