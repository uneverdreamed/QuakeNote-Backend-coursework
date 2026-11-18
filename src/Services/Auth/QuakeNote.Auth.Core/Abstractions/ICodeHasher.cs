namespace QuakeNote.Auth.Core.Abstractions;

public interface ICodeHasher
{
    string Hash(string code);
    bool Verify(string code, string hash);
}