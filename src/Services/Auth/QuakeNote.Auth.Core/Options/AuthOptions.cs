using QuakeNote.Shared.Contracts;

namespace QuakeNote.Auth.Core.Options;

public sealed class AuthOptions
{
    public const int CodeLength = 6;

    public int CodeExpiryMinutes { get; set; } = 10;
    public short MaxAttempts { get; set; } = 5;
    public int RequestCooldownSeconds { get; set; } = 60;

    /// <summary>Email-адреса, которым при входе назначается роль admin.</summary>
    public string[] AdminEmails { get; set; } = [];
}

public sealed class JwtOptions
{
    public string Issuer { get; set; } = JwtDefaults.Issuer;
    public string Audience { get; set; } = JwtDefaults.Audience;
    public int ExpiresMinutes { get; set; } = 60;
    public string? PrivateKey { get; set; }
    public string? PrivateKeyPath { get; set; }
}