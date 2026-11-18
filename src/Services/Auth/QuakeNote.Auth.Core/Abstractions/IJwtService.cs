using QuakeNote.Auth.Data.Entities;

namespace QuakeNote.Auth.Core.Abstractions;

public sealed record JwtToken(string Token, DateTimeOffset ExpiresAt, string Jti);

public interface IJwtService
{
    JwtToken CreateToken(User user);
}