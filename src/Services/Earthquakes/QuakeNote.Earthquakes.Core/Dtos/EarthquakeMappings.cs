using System.Linq.Expressions;
using QuakeNote.Earthquakes.Core.Infrastructure;
using QuakeNote.Earthquakes.Data.Entities;
using QuakeNote.Shared.Contracts;

namespace QuakeNote.Earthquakes.Core.Dtos;

public static class EarthquakeMappings
{
    /// <summary>Проекция для списка: выбираются только нужные колонки.</summary>
    public static readonly Expression<Func<Earthquake, EarthquakeListItemDto>> ToListItemExpression = e =>
        new EarthquakeListItemDto(
            e.Uid, e.ExternalId, e.EpicenterLat, e.EpicenterLon, e.Depth, e.Magnitude, e.OccurredAt,
            e.RegionName, e.Source,
            e.Assessment != null ? e.Assessment.VerificationStatus : EventVerificationStatuses.InsufficientData,
            e.Assessment != null ? e.Assessment.ReportsCount : 0);

    /// <remarks>Нужны загруженные Assessment и Readings (с Station).</remarks>
    public static EarthquakeDetailDto ToDetailDto(this Earthquake e) => new(
        e.Uid, e.ExternalId, e.EpicenterLat, e.EpicenterLon, e.Depth, e.Magnitude, e.OccurredAt,
        e.RegionName, e.Source,
        e.Assessment?.ToDto(),
        e.Readings.OrderBy(r => r.DistanceToEpicenterKm).Select(r => r.ToDto()).ToList());

    public static EventAssessmentDto ToDto(this EventAssessment a) => new(
        a.InstrumentalIntensity, a.MacroseismicIntensity, a.VerificationStatus, a.ReportsCount, a.LastRecomputedAt);

    /// <remarks>Нужна загруженная Station.</remarks>
    public static StationReadingDto ToDto(this StationReading r) => new(
        r.Station.Uid, r.Station.Code, r.Station.Name, r.Station.Latitude, r.Station.Longitude,
        r.Pga, r.Pgv, r.Pgd, r.AriasIntensity, r.PWaveArrivalAt, r.DistanceToEpicenterKm);

    public static StationDto ToDto(this SeismicStation s) => new(
        s.Uid, s.Code, s.Name, s.Latitude, s.Longitude, s.Region, s.Organization);

    /// <summary>Переносит поля события из запроса в сущность (внешний id и регион обрабатываются отдельно).</summary>
    public static void ApplyTo(this IEarthquakeInput dto, Earthquake e)
    {
        e.EpicenterLat = dto.EpicenterLat!.Value;
        e.EpicenterLon = dto.EpicenterLon!.Value;
        e.Depth = dto.Depth!.Value;
        e.Magnitude = dto.Magnitude!.Value;
        e.OccurredAt = dto.OccurredAt!.Value.ToUtc();
        e.RegionName = dto.RegionName!.Trim();
        e.Source = Clean(dto.Source) ?? "manual";
    }

    public static void ApplyTo(this IStationInput dto, SeismicStation s)
    {
        s.Code = dto.Code!.Trim();
        s.Name = dto.Name!.Trim();
        s.Latitude = dto.Latitude!.Value;
        s.Longitude = dto.Longitude!.Value;
        s.Region = dto.Region!.Trim();
        s.Organization = dto.Organization!.Trim();
    }

    public static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}