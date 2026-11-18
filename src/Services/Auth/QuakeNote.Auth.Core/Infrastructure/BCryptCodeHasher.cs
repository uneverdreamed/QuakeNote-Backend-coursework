using QuakeNote.Auth.Core.Abstractions;

namespace QuakeNote.Auth.Core.Infrastructure;

public sealed class BCryptCodeHasher : ICodeHasher
{
    public string Hash(string code) => BCrypt.Net.BCrypt.HashPassword(code);

    public bool Verify(string code, string hash) => BCrypt.Net.BCrypt.Verify(code, hash);
}