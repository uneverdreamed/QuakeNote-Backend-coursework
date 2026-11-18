namespace QuakeNote.Auth.Data.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }

    public User User { get; set; } = null!;
}