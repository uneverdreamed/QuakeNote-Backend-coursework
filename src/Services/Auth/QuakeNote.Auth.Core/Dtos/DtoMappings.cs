using QuakeNote.Auth.Data.Entities;

namespace QuakeNote.Auth.Core.Dtos;

internal static class DtoMappings
{
    public static MeDto ToMeDto(this User user)
        => new(user.Uid, user.Email, user.Role, user.CreatedAt);

    public static ProfileDto ToDto(this UserProfile profile)
        => new(profile.FirstName, profile.LastName, profile.Location);

    public static SettingsDto ToDto(this UserSettings s)
        => new(s.NotificationsEnabled, s.MinMagnitude, s.ReminderEnabled,
               s.DigestEnabled, s.EmailNotifications, s.DefaultRegion);

    public static string? NullIfBlank(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}