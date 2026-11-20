using Microsoft.EntityFrameworkCore;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Infrastructure;
using QuakeNote.Earthquakes.Data;
using QuakeNote.Earthquakes.Data.Entities;
using QuakeNote.Shared.Contracts.Errors;

namespace QuakeNote.Earthquakes.Core.Services;

public class StationReadingService(EarthquakesDbContext db, EarthquakeCache cache)
{
    /// <summary>Загрузить показания: записи станций, которые уже есть, обновляются, остальные добавляются.</summary>
    public async Task<List<StationReadingDto>> UpsertAsync(
        Guid earthquakeUid, UpsertReadingsDto dto, CancellationToken ct = default)
    {
        var earthquake = await db.Earthquakes.Include(e => e.Readings)
            .FirstOrDefaultAsync(e => e.Uid == earthquakeUid, ct)
            ?? throw new NotFoundException("Событие не найдено.", "earthquake_not_found");

        var inputs = dto.Readings!;
        var stationUids = inputs.Select(r => r.StationUid!.Value).ToList();
        var stations = await db.Stations.Where(s => stationUids.Contains(s.Uid)).ToListAsync(ct);

        var unknown = stationUids.Except(stations.Select(s => s.Uid)).ToList();
        if (unknown.Count > 0)
            throw new BadRequestException(
                $"Неизвестные станции: {string.Join(", ", unknown)}.", "unknown_station");

        foreach (var input in inputs)
        {
            var station = stations.First(s => s.Uid == input.StationUid);
            var reading = earthquake.Readings.FirstOrDefault(r => r.StationId == station.Id);
            if (reading is null)
            {
                reading = new StationReading { EarthquakeId = earthquake.Id, StationId = station.Id };
                db.Readings.Add(reading);
            }

            reading.Pga = input.Pga!.Value;
            reading.Pgv = input.Pgv!.Value;
            reading.Pgd = input.Pgd;
            reading.AriasIntensity = input.AriasIntensity;
            reading.PWaveArrivalAt = input.PWaveArrivalAt!.Value.ToUtc();
            reading.DistanceToEpicenterKm = input.DistanceToEpicenterKm
                ?? GeoMath.HaversineKm(earthquake.EpicenterLat, earthquake.EpicenterLon,
                                       station.Latitude, station.Longitude);
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException(
                "Показания для этих станций уже загружаются другим запросом.", "readings_conflict");
        }

        await cache.InvalidateDetailAsync(earthquakeUid);
        return await GetByEarthquakeAsync(earthquakeUid, ct);
    }

    /// <summary>Показания всех станций для события, ближайшие к эпицентру первыми (используется и gRPC-сервером).</summary>
    public async Task<List<StationReadingDto>> GetByEarthquakeAsync(Guid earthquakeUid, CancellationToken ct = default)
    {
        if (!await db.Earthquakes.AnyAsync(e => e.Uid == earthquakeUid, ct))
            throw new NotFoundException("Событие не найдено.", "earthquake_not_found");

        var readings = await db.Readings.AsNoTracking()
            .Include(r => r.Station)
            .Where(r => r.Earthquake.Uid == earthquakeUid)
            .OrderBy(r => r.DistanceToEpicenterKm)
            .ToListAsync(ct);

        return readings.Select(r => r.ToDto()).ToList();
    }
}