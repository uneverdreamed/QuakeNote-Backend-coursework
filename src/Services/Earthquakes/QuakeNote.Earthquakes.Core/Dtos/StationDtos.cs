namespace QuakeNote.Earthquakes.Core.Dtos;

public interface IStationInput
{
    string? Code { get; }
    string? Name { get; }
    decimal? Latitude { get; }
    decimal? Longitude { get; }
    string? Region { get; }
    string? Organization { get; }
}

/// <summary>Создание станции. Код уникален.</summary>
public sealed record CreateStationDto(
    string? Code, string? Name, decimal? Latitude, decimal? Longitude, string? Region, string? Organization)
    : IStationInput;

/// <summary>Изменение станции (полная замена полей).</summary>
public sealed record UpdateStationDto(
    string? Code, string? Name, decimal? Latitude, decimal? Longitude, string? Region, string? Organization)
    : IStationInput;

public sealed record StationDto(
    Guid Uid, string Code, string Name, decimal Latitude, decimal Longitude, string Region, string Organization);

/// <summary>Фильтр списка станций по области (все четыре значения сразу).</summary>
public class StationsFilterDto : IBboxFilter
{
    public decimal? MinLat { get; set; }
    public decimal? MaxLat { get; set; }
    public decimal? MinLon { get; set; }
    public decimal? MaxLon { get; set; }
}