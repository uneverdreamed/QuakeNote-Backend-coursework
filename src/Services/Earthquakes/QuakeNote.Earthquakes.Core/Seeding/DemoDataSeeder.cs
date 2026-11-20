using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuakeNote.Earthquakes.Core.Infrastructure;
using QuakeNote.Earthquakes.Data;
using QuakeNote.Earthquakes.Data.Entities;

namespace QuakeNote.Earthquakes.Core.Seeding;

/// <summary>
/// Демонстрационные данные для разработки и показа. Всё синтетическое:
/// станции и события выдуманы, показания считаются по упрощённой формуле затухания.
/// </summary>
public class DemoDataSeeder(EarthquakesDbContext db, TimeProvider time, ILogger<DemoDataSeeder> logger)
{
    private const string StationPrefix = "DEMO-";
    private const string Source = "demo-seed";
    private const string Organization = "Демо-данные";
    private const decimal ReadingRadiusKm = 400m;

    private sealed record StationSeed(string Code, string Name, decimal Lat, decimal Lon, string Region);

    private sealed record EventSeed(
        string ExternalId, decimal Lat, decimal Lon, decimal Depth, decimal Magnitude, TimeSpan Ago, string Region);

    private static readonly StationSeed[] Stations =
    [
        new("DEMO-01", "Демо-станция 1", 43.60m, 39.73m, "Черноморское побережье"),
        new("DEMO-02", "Демо-станция 2", 43.35m, 42.45m, "Северный Кавказ"),
        new("DEMO-03", "Демо-станция 3", 42.98m, 47.50m, "Дагестан"),
        new("DEMO-04", "Демо-станция 4", 51.85m, 104.85m, "Байкал"),
        new("DEMO-05", "Демо-станция 5", 52.70m, 103.60m, "Байкал"),
        new("DEMO-06", "Демо-станция 6", 53.05m, 158.65m, "Камчатка"),
        new("DEMO-07", "Демо-станция 7", 52.90m, 160.20m, "Камчатка")
    ];

    private static readonly EventSeed[] Events =
    [
        new("demo-001", 43.40m, 42.10m, 12m, 3.8m, TimeSpan.FromHours(3), "Северный Кавказ"),
        new("demo-002", 43.10m, 46.90m, 8m, 4.4m, TimeSpan.FromDays(1), "Дагестан"),
        new("demo-003", 52.10m, 104.20m, 15m, 4.9m, TimeSpan.FromDays(3), "Байкал"),
        new("demo-004", 52.95m, 159.40m, 35m, 5.6m, TimeSpan.FromDays(7), "Камчатка"),
        new("demo-005", 43.50m, 39.90m, 10m, 3.2m, TimeSpan.FromDays(20), "Черноморское побережье")
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Stations.AnyAsync(s => s.Code.StartsWith(StationPrefix), ct))
        {
            logger.LogInformation("Демо-данные уже загружены, пропускаем.");
            return;
        }

        var now = time.GetUtcNow().UtcDateTime;

        var stations = Stations.Select(s => new SeismicStation
        {
            Code = s.Code,
            Name = s.Name,
            Latitude = s.Lat,
            Longitude = s.Lon,
            Region = s.Region,
            Organization = Organization
        }).ToList();
        db.Stations.AddRange(stations);

        foreach (var seed in Events)
        {
            var occurredAt = now - seed.Ago;
            var earthquake = new Earthquake
            {
                ExternalId = seed.ExternalId,
                EpicenterLat = seed.Lat,
                EpicenterLon = seed.Lon,
                Depth = seed.Depth,
                Magnitude = seed.Magnitude,
                OccurredAt = occurredAt,
                RegionName = seed.Region,
                Source = Source,
                Assessment = new EventAssessment { LastRecomputedAt = now }
            };
            db.Earthquakes.Add(earthquake);

            foreach (var station in stations)
            {
                var distance = GeoMath.HaversineKm(seed.Lat, seed.Lon, station.Latitude, station.Longitude);
                if (distance > ReadingRadiusKm)
                    continue;

                // Упрощённое затухание: растёт с магнитудой, убывает с расстоянием. Не физическая модель.
                var pga = Math.Pow(10, 0.45 * (double)seed.Magnitude) * 12 / Math.Pow((double)distance + 15, 1.2);
                var pgv = pga * 0.12;

                db.Readings.Add(new StationReading
                {
                    Earthquake = earthquake,
                    Station = station,
                    Pga = Math.Max(0.01m, Math.Round((decimal)pga, 2)),
                    Pgv = Math.Max(0.01m, Math.Round((decimal)pgv, 2)),
                    PWaveArrivalAt = occurredAt.AddSeconds((double)distance / 6.0),
                    DistanceToEpicenterKm = distance
                });
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Загружены демо-данные: {Stations} станций, {Events} событий.", stations.Count, Events.Length);
    }
}