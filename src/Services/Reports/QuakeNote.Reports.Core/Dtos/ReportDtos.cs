namespace QuakeNote.Reports.Core.Dtos;

/// <summary>Общие поля базового отчёта (для общего валидатора).</summary>
public interface IReportInput
{
    decimal? Latitude { get; }
    decimal? Longitude { get; }
    int? FeltIntensity { get; }
    string? RegionName { get; }
}

/// <summary>Создание базового отчёта.</summary>
/// <param name="Latitude">Широта, -90..90.</param>
/// <param name="Longitude">Долгота, -180..180.</param>
/// <param name="FeltIntensity">Ощущаемая интенсивность по MSK-64, 1..10.</param>
/// <param name="RegionName">Название региона (необязательно, до 50 символов).</param>
public sealed record CreateReportDto(decimal? Latitude, decimal? Longitude, int? FeltIntensity, string? RegionName = null)
    : IReportInput;

/// <summary>Редактирование базового отчёта (полная замена полей).</summary>
public sealed record UpdateReportDto(decimal? Latitude, decimal? Longitude, int? FeltIntensity, string? RegionName = null)
    : IReportInput;

/// <summary>Отчёт глазами его владельца.</summary>
public record ReportDto(
    Guid Uid,
    decimal Latitude,
    decimal Longitude,
    int FeltIntensity,
    string? RegionName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string ValidationStatus,
    decimal? ExpectedIntensity,
    decimal? Deviation,
    Guid? EarthquakeUid,
    DateTime? MatchedAt,
    bool HasDetails);

/// <summary>Отчёт вместе с расширенной анкетой.</summary>
public sealed record ReportWithDetailsDto : ReportDto
{
    public ReportWithDetailsDto(ReportDto source, DetailedReportDto? details) : base(source)
        => Details = details;

    public DetailedReportDto? Details { get; init; }
}

/// <summary>Публичное представление отчёта: без user_uid и идентификатора отчёта.</summary>
public sealed record AnonymizedReportDto(
    decimal Latitude,
    decimal Longitude,
    int FeltIntensity,
    string ValidationStatus,
    DateTime CreatedAt);

/// <summary>Результат верификации (internal PATCH от Verification).</summary>
/// <param name="ValidationStatus">verified | outlier | unverified.</param>
/// <param name="ExpectedIntensity">Расчётная интенсивность (обязательна для verified и outlier).</param>
/// <param name="Deviation">Отклонение заявленной интенсивности от расчётной.</param>
/// <param name="MatchedEarthquakeUid">Событие, к которому привязан отчёт.</param>
public sealed record MatchReportDto(
    string ValidationStatus,
    decimal? ExpectedIntensity,
    decimal? Deviation,
    Guid? MatchedEarthquakeUid);

/// <summary>Статистика отчётов по событию (для Earthquakes, через gRPC).</summary>
/// <param name="MeanIntensity">Средняя ощущаемая интенсивность по отчётам без выбросов.</param>
public sealed record ReportsStatsDto(
    Guid EarthquakeUid,
    int TotalCount,
    int VerifiedCount,
    int OutlierCount,
    int UnverifiedCount,
    int PendingCount,
    double? MeanIntensity);

/// <summary>Несопоставленный отчёт (internal, для Verification при появлении нового события).</summary>
public sealed record UnmatchedReportDto(
    Guid Uid,
    Guid? UserUid,
    decimal Latitude,
    decimal Longitude,
    int FeltIntensity,
    DateTime CreatedAt);