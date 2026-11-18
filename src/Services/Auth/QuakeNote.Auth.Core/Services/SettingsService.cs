using Microsoft.EntityFrameworkCore;
using QuakeNote.Auth.Core.Dtos;
using QuakeNote.Auth.Data;
using QuakeNote.Shared.Contracts.Errors;

namespace QuakeNote.Auth.Core.Services;

public class SettingsService(AuthDbContext db)
{
    public async Task<SettingsDto> GetAsync(Guid userUid, CancellationToken ct = default)
    {
        var settings = await db.UserSettings.AsNoTracking()
                           .FirstOrDefaultAsync(s => s.User.Uid == userUid, ct)
                       ?? throw new NotFoundException("Настройки не найдены.", "settings_not_found");
        return settings.ToDto();
    }

    public async Task<SettingsDto> UpdateAsync(Guid userUid, UpdateSettingsDto dto, CancellationToken ct = default)
    {
        var settings = await db.UserSettings.FirstOrDefaultAsync(s => s.User.Uid == userUid, ct)
                       ?? throw new NotFoundException("Настройки не найдены.", "settings_not_found");

        settings.NotificationsEnabled = dto.NotificationsEnabled;
        settings.MinMagnitude = dto.MinMagnitude;
        settings.ReminderEnabled = dto.ReminderEnabled;
        settings.DigestEnabled = dto.DigestEnabled;
        settings.EmailNotifications = dto.EmailNotifications;
        settings.DefaultRegion = dto.DefaultRegion.NullIfBlank();

        await db.SaveChangesAsync(ct);
        return settings.ToDto();
    }
}