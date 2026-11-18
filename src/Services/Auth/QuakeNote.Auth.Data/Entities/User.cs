using QuakeNote.Shared.Contracts;

namespace QuakeNote.Auth.Data.Entities;

public class User
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.User;
    public DateOnly BirthDate { get; set; }
    public DateTimeOffset TermsAcceptedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLogin { get; set; }

    public UserProfile? Profile { get; set; }
    public UserSettings? Settings { get; set; }
}