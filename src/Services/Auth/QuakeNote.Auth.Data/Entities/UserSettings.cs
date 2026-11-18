namespace QuakeNote.Auth.Data.Entities;

public class UserSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
    public short MinMagnitude { get; set; } = 4;
    public bool ReminderEnabled { get; set; } = true;
    public bool DigestEnabled { get; set; } = false;
    public bool EmailNotifications { get; set; } = false;
    public string? DefaultRegion { get; set; }

    public User User { get; set; } = null!;
}