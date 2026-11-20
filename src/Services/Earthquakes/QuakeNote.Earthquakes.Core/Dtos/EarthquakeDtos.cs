using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using QuakeNote.Earthquakes.Core.Infrastructure;

namespace QuakeNote.Earthquakes.Core.Dtos;

/// <summary>Общие поля события при создании и изменении (для общего валидатора).</summary>
public interface IEarthquakeInput
{
    decimal? EpicenterLat { get; }
    decimal? EpicenterLon { get; }
    decimal? Depth { get; }
    decimal? Magnitude { get; }
    DateTime? OccurredAt { get; }
    string? RegionName { get; }
    string? Source { get; }
    string? ExternalId { get; }
}

/// <summary>Создание события.</summary>
/// <param name="Depth">Глубина очага, км (≥ 0).</param>
/// <param name="Magnitude">Магнитуда, 0..10.</param>
/// <param name="Source">Источник данных (по умолчанию «manual»).</param>
/// <param name="ExternalId">Идентификатор у источника; если не задан, будет сгенерирован.</param>
public sealed record CreateEarthquakeDto(
    decimal? EpicenterLat, decimal? EpicenterLon, decimal? Depth, decimal? Magnitude,
    DateTime? OccurredAt, string? RegionName, string? Source = null, string? ExternalId = null) : IEarthquakeInput;

/// <summary>Изменение события (полная замена полей).</summary>
public sealed record UpdateEarthquakeDto(
    decimal? EpicenterLat, decimal? EpicenterLon, decimal? Depth, decimal? Magnitude,
    DateTime? OccurredAt, string? RegionName, string? Source = null, string? ExternalId = null) : IEarthquakeInput;

public interface IBboxFilter
{
    decimal? MinLat { get; }
    decimal? MaxLat { get; }
    decimal? MinLon { get; }
    decimal? MaxLon { get; }
}

/// <summary>Фильтры списка событий. Область (bbox) задаётся четырьмя значениями сразу.</summary>
public class EarthquakesFilterDto : IBboxFilter
{
    public decimal? MinLat { get; set; }
    public decimal? MaxLat { get; set; }
    public decimal? MinLon { get; set; }
    public decimal? MaxLon { get; set; }

    /// <summary>Начало периода (включительно).</summary>
    public DateTime? From { get; set; }

    /// <summary>Конец периода (включительно).</summary>
    public DateTime? To { get; set; }

    public decimal? MinMagnitude { get; set; }
    public decimal? MaxMagnitude { get; set; }

    /// <summary>Номер страницы, с 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Размер страницы, 1..100.</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>Короткий хэш нормализованных фильтров: часть ключа в Redis.</summary>
    public string ToCacheHash()
    {
        var inv = CultureInfo.InvariantCulture;
        var raw = string.Join('|',
            MinLat?.ToString(inv), MaxLat?.ToString(inv), MinLon?.ToString(inv), MaxLon?.ToString(inv),
            From?.ToUtc().Ticks, To?.ToUtc().Ticks,
            MinMagnitude?.ToString(inv), MaxMagnitude?.ToString(inv),
            Page, PageSize);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant()[..16];
    }
}

public sealed record EventAssessmentDto(
    decimal? InstrumentalIntensity,
    decimal? MacroseismicIntensity,
    string VerificationStatus,
    int ReportsCount,
    DateTime LastRecomputedAt);

/// <summary>Событие в списке.</summary>
public sealed record EarthquakeListItemDto(
    Guid Uid,
    string ExternalId,
    decimal EpicenterLat,
    decimal EpicenterLon,
    decimal Depth,
    decimal Magnitude,
    DateTime OccurredAt,
    string RegionName,
    string Source,
    string VerificationStatus,
    int ReportsCount);

/// <summary>Событие целиком: параметры, оценка и показания станций.</summary>
public sealed record EarthquakeDetailDto(
    Guid Uid,
    string ExternalId,
    decimal EpicenterLat,
    decimal EpicenterLon,
    decimal Depth,
    decimal Magnitude,
    DateTime OccurredAt,
    string RegionName,
    string Source,
    EventAssessmentDto? Assessment,
    List<StationReadingDto> Readings);