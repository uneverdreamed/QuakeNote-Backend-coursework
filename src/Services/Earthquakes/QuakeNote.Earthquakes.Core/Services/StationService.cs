using Microsoft.EntityFrameworkCore;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Infrastructure;
using QuakeNote.Earthquakes.Data;
using QuakeNote.Earthquakes.Data.Entities;
using QuakeNote.Shared.Contracts.Errors;

namespace QuakeNote.Earthquakes.Core.Services;

public class StationService(EarthquakesDbContext db, EarthquakeCache cache)
{
    public async Task<List<StationDto>> ListAsync(StationsFilterDto f, CancellationToken ct = default)
    {
        var query = db.Stations.AsNoTracking().AsQueryable();

        if (f.MinLat is { } minLat && f.MaxLat is { } maxLat && f.MinLon is { } minLon && f.MaxLon is { } maxLon)
            query = query.Where(s => s.Latitude >= minLat && s.Latitude <= maxLat
                                  && s.Longitude >= minLon && s.Longitude <= maxLon);

        return await query.OrderBy(s => s.Code)
            .Select(s => new StationDto(s.Uid, s.Code, s.Name, s.Latitude, s.Longitude, s.Region, s.Organization))
            .ToListAsync(ct);
    }

    public async Task<StationDto> GetAsync(Guid uid, CancellationToken ct = default)
        => (await FindAsync(uid, ct)).ToDto();

    public async Task<StationDto> CreateAsync(CreateStationDto dto, CancellationToken ct = default)
    {
        var code = dto.Code!.Trim();
        if (await db.Stations.AnyAsync(s => s.Code == code, ct))
            throw CodeExists();

        var station = new SeismicStation();
        dto.ApplyTo(station);
        db.Stations.Add(station);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw CodeExists();
        }

        return station.ToDto();
    }

    public async Task<StationDto> UpdateAsync(Guid uid, UpdateStationDto dto, CancellationToken ct = default)
    {
        var station = await FindAsync(uid, ct);

        var newCode = dto.Code!.Trim();
        if (newCode != station.Code && await db.Stations.AnyAsync(s => s.Code == newCode, ct))
            throw CodeExists();

        dto.ApplyTo(station);

        // Станция участвует в показаниях: расстояния могли измениться, а кэш деталей событий устарел
        var readings = await db.Readings.Include(r => r.Earthquake)
            .Where(r => r.StationId == station.Id).ToListAsync(ct);
        foreach (var reading in readings)
        {
            reading.DistanceToEpicenterKm = GeoMath.HaversineKm(
                reading.Earthquake.EpicenterLat, reading.Earthquake.EpicenterLon,
                station.Latitude, station.Longitude);
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw CodeExists();
        }

        await cache.InvalidateDetailsAsync(readings.Select(r => r.Earthquake.Uid));
        return station.ToDto();
    }

    public async Task DeleteAsync(Guid uid, CancellationToken ct = default)
    {
        var station = await FindAsync(uid, ct);

        if (await db.Readings.AnyAsync(r => r.StationId == station.Id, ct))
            throw new ConflictException(
                "Нельзя удалить станцию, у которой есть показания.", "station_has_readings");

        db.Stations.Remove(station);
        await db.SaveChangesAsync(ct);
    }

    private async Task<SeismicStation> FindAsync(Guid uid, CancellationToken ct)
        => await db.Stations.FirstOrDefaultAsync(s => s.Uid == uid, ct)
           ?? throw new NotFoundException("Станция не найдена.", "station_not_found");

    private static ConflictException CodeExists()
        => new("Станция с таким кодом уже существует.", "station_code_exists");
}