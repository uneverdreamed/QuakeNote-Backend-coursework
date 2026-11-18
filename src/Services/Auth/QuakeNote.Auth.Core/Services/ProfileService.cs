using Microsoft.EntityFrameworkCore;
using QuakeNote.Auth.Core.Dtos;
using QuakeNote.Auth.Data;
using QuakeNote.Shared.Contracts.Errors;

namespace QuakeNote.Auth.Core.Services;

public class ProfileService(AuthDbContext db)
{
    public async Task<ProfileDto> GetAsync(Guid userUid, CancellationToken ct = default)
    {
        var profile = await db.UserProfiles.AsNoTracking()
                          .FirstOrDefaultAsync(p => p.User.Uid == userUid, ct)
                      ?? throw new NotFoundException("Профиль не найден.", "profile_not_found");
        return profile.ToDto();
    }

    public async Task<ProfileDto> UpdateAsync(Guid userUid, UpdateProfileDto dto, CancellationToken ct = default)
    {
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.User.Uid == userUid, ct)
                      ?? throw new NotFoundException("Профиль не найден.", "profile_not_found");

        profile.FirstName = dto.FirstName.NullIfBlank();
        profile.LastName = dto.LastName.NullIfBlank();
        profile.Location = dto.Location.NullIfBlank();

        await db.SaveChangesAsync(ct);
        return profile.ToDto();
    }
}