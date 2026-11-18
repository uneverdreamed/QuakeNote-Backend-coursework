using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using QuakeNote.Auth.Core.Abstractions;
using QuakeNote.Auth.Core.Options;
using QuakeNote.Auth.Data.Entities;

namespace QuakeNote.Auth.Core.Infrastructure;

public sealed class JwtService : IJwtService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _credentials;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        var pem = _options.PrivateKey;
        if (string.IsNullOrWhiteSpace(pem) && !string.IsNullOrWhiteSpace(_options.PrivateKeyPath)
            && File.Exists(_options.PrivateKeyPath))
        {
            pem = File.ReadAllText(_options.PrivateKeyPath);
        }

        if (string.IsNullOrWhiteSpace(pem))
            throw new InvalidOperationException("Jwt:PrivateKey or Jwt:PrivateKeyPath must be configured.");

        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.Replace("\\n", "\n"));
        _credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
    }

    public JwtToken CreateToken(User user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.ExpiresMinutes);
        var jti = Guid.NewGuid().ToString("N");

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            SigningCredentials = _credentials,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = user.Uid.ToString(),
                ["jti"] = jti,
                ["email"] = user.Email,
                ["role"] = user.Role
            }
        };

        return new JwtToken(_handler.CreateToken(descriptor), new DateTimeOffset(expires), jti);
    }
}