using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Infrastructure;
using QuakeNote.Earthquakes.Data;
using QuakeNote.Earthquakes.Data.Entities;
using QuakeNote.Shared.Contracts.Dtos;
using QuakeNote.Shared.Contracts.Errors;
using QuakeNote.Shared.Messaging;

namespace QuakeNote.Earthquakes.Core.Services;

public class EarthquakeService(
    EarthquakesDbContext db,
    EarthquakeCache cache,
    IMessagePublisher publisher,
    TimeProvider time,
    ILogger<EarthquakeService> logger)
{
    /// <summary>Список событий. Результат кэшируется в Redis по хэшу фильтров (cache-aside).</summary>
    public Task<PagedResultDto<EarthquakeListItemDto>> ListAsync(EarthquakesFilterDto filter, CancellationToken ct = default)
        => cache.GetListAsync(filter.ToCacheHash(), () => QueryListAsync(filter, ct));

    /// <summary>Событие целиком. Кэшируется в Redis по uid.</summary>
    public Task<EarthquakeDetailDto> GetAsync(Guid uid, CancellationToken ct = default)
        => cache.GetDetailAsync(uid, () => LoadDetailAsync(uid, ct));

    public async Task<EarthquakeDetailDto> CreateAsync(CreateEarthquakeDto dto, CancellationToken ct = default)
    {
        var externalId = EarthquakeMappings.Clean(dto.ExternalId) ?? $"manual-{Guid.NewGuid():N}"[..19];
        if (await db.Earthquakes.AnyAsync(e => e.ExternalId == externalId, ct))
            throw new ConflictException("Событие с таким внешним идентификатором уже есть.", "external_id_exists");

        var earthquake = new Earthquake
        {
            ExternalId = externalId,
            Assessment = new EventAssessment { LastRecomputedAt = time.GetUtcNow().UtcDateTime }
        };
        dto.ApplyTo(earthquake);

        db.Earthquakes.Add(earthquake);
        await SaveAsync(ct);

        await cache.InvalidateListAsync();
        await publisher.PublishBestEffortAsync(logger, earthquake.ToCreatedEvent(), ct);
        return earthquake.ToDetailDto();
    }

    public async Task<EarthquakeDetailDto> UpdateAsync(Guid uid, UpdateEarthquakeDto dto, CancellationToken ct = default)
    {
        var earthquake = await db.Earthquakes
            .Include(e => e.Assessment)
            .Include(e => e.Readings).ThenInclude(r => r.Station)
            .FirstOrDefaultAsync(e => e.Uid == uid, ct)
            ?? throw NotFound();

        var newExternalId = EarthquakeMappings.Clean(dto.ExternalId);
        if (newExternalId is not null && newExternalId != earthquake.ExternalId)
        {
            if (await db.Earthquakes.AnyAsync(e => e.ExternalId == newExternalId, ct))
                throw new ConflictException("Событие с таким внешним идентификатором уже есть.", "external_id_exists");
            earthquake.ExternalId = newExternalId;
        }

        var epicenterChanged = earthquake.EpicenterLat != dto.EpicenterLat || earthquake.EpicenterLon != dto.EpicenterLon;
        dto.ApplyTo(earthquake);
        if (epicenterChanged)
            ReadingDistances.Recalculate(earthquake);

        await SaveAsync(ct);

        await cache.InvalidateListAsync();
        await cache.InvalidateDetailAsync(uid);
        return earthquake.ToDetailDto();
    }

    /// <summary>Удаляет событие; показания и оценка удаляются каскадно.</summary>
    public async Task DeleteAsync(Guid uid, CancellationToken ct = default)
    {
        var earthquake = await db.Earthquakes.FirstOrDefaultAsync(e => e.Uid == uid, ct) ?? throw NotFound();

        db.Earthquakes.Remove(earthquake);
        await db.SaveChangesAsync(ct);

        await cache.InvalidateListAsync();
        await cache.InvalidateDetailAsync(uid);
    }

    private async Task<PagedResultDto<EarthquakeListItemDto>> QueryListAsync(EarthquakesFilterDto f, CancellationToken ct)
    {
        var query = db.Earthquakes.AsNoTracking().AsQueryable();

        if (f.MinLat is { } minLat && f.MaxLat is { } maxLat && f.MinLon is { } minLon && f.MaxLon is { } maxLon)
            query = query.Where(e => e.EpicenterLat >= minLat && e.EpicenterLat <= maxLat
                                  && e.EpicenterLon >= minLon && e.EpicenterLon <= maxLon);

        if (f.From is { } fromDate)
        {
            var utc = fromDate.ToUtc();
            query = query.Where(e => e.OccurredAt >= utc);
        }

        if (f.To is { } toDate)
        {
            var utc = toDate.ToUtc();
            query = query.Where(e => e.OccurredAt <= utc);
        }

        if (f.MinMagnitude is { } minMagnitude)
            query = query.Where(e => e.Magnitude >= minMagnitude);

        if (f.MaxMagnitude is { } maxMagnitude)
            query = query.Where(e => e.Magnitude <= maxMagnitude);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.OccurredAt).ThenByDescending(e => e.Id)
            .Skip((f.Page - 1) * f.PageSize).Take(f.PageSize)
            .Select(EarthquakeMappings.ToListItemExpression)
            .ToListAsync(ct);

        return new PagedResultDto<EarthquakeListItemDto>(items, f.Page, f.PageSize, total);
    }

    private async Task<EarthquakeDetailDto> LoadDetailAsync(Guid uid, CancellationToken ct)
    {
        var earthquake = await db.Earthquakes.AsNoTracking()
            .Include(e => e.Assessment)
            .Include(e => e.Readings).ThenInclude(r => r.Station)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Uid == uid, ct)
            ?? throw NotFound();

        return earthquake.ToDetailDto();
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // два одновременных запроса с одним внешним идентификатором
            throw new ConflictException("Событие с таким внешним идентификатором уже есть.", "external_id_exists");
        }
    }

    private static NotFoundException NotFound() => new("Событие не найдено.", "earthquake_not_found");
}