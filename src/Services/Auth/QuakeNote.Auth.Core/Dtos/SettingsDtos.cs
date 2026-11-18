namespace QuakeNote.Auth.Core.Dtos;

public record SettingsDto(
    bool NotificationsEnabled,
    short MinMagnitude,
    bool ReminderEnabled,
    bool DigestEnabled,
    bool EmailNotifications,
    string? DefaultRegion);

public record UpdateSettingsDto(
    bool NotificationsEnabled,
    short MinMagnitude,
    bool ReminderEnabled,
    bool DigestEnabled,
    bool EmailNotifications,
    string? DefaultRegion);