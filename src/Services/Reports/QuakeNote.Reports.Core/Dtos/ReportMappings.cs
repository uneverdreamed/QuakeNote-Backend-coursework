using System.Linq.Expressions;
using QuakeNote.Reports.Data.Entities;

namespace QuakeNote.Reports.Core.Dtos;

public static class ReportMappings
{
    /// <summary>Проекция для запросов к БД (EF переведёт её в SELECT нужных колонок).</summary>
    public static readonly Expression<Func<Report, ReportDto>> ToDtoExpression = r => new ReportDto(
        r.Uid, r.Latitude, r.Longitude, r.FeltIntensity, r.RegionName, r.CreatedAt, r.UpdatedAt,
        r.ValidationStatus, r.ExpectedIntensity, r.Deviation, r.EarthquakeUid, r.MatchedAt, r.Detail != null);

    public static readonly Expression<Func<Report, AnonymizedReportDto>> ToAnonymizedExpression = r =>
        new AnonymizedReportDto(r.Latitude, r.Longitude, r.FeltIntensity, r.ValidationStatus, r.CreatedAt);

    private static readonly Func<Report, ReportDto> ToDtoCompiled = ToDtoExpression.Compile();

    /// <remarks>Для корректного HasDetails сущность должна быть загружена с Include(r => r.Detail).</remarks>
    public static ReportDto ToDto(this Report report) => ToDtoCompiled(report);

    public static ReportWithDetailsDto ToDtoWithDetails(this Report report)
        => new(report.ToDto(), report.Detail?.ToDto());

    public static DetailedReportDto ToDto(this DetailedReport d) => new()
    {
        EventDate = d.EventDate,
        Duration = d.Duration,
        LocationType = d.LocationType,
        HumanAction = d.HumanAction,
        OscillationType = d.OscillationType,
        Reactions = [.. d.Reactions],
        WitnessesCount = d.WitnessesCount,
        AffectedObjects = [.. d.AffectedObjects],
        BuildingElements = [.. d.BuildingElements],
        ObservationsInside = [.. d.ObservationsInside],
        OutdoorObjects = [.. d.OutdoorObjects],
        Infrastructure = [.. d.Infrastructure],
        SurfacePhenomena = [.. d.SurfacePhenomena],
        CrackWidth = d.CrackWidth,
        CrackLength = d.CrackLength,
        OtherObservations = [.. d.OtherObservations],
        ContactPhone = d.ContactPhone,
        ContactEmail = d.ContactEmail,
        ContactName = d.ContactName,
        AdditionalInfo = d.AdditionalInfo
    };

    /// <summary>Переносит значения анкеты в сущность: пустые строки → null, списки очищаются от пустых и дублей.</summary>
    public static void ApplyTo(this DetailedReportDto dto, DetailedReport e)
    {
        e.EventDate = dto.EventDate is { } date ? ToUtc(date) : null;
        e.Duration = Clean(dto.Duration);
        e.LocationType = Clean(dto.LocationType);
        e.HumanAction = Clean(dto.HumanAction);
        e.OscillationType = Clean(dto.OscillationType);
        e.Reactions = CleanList(dto.Reactions);
        e.WitnessesCount = Clean(dto.WitnessesCount);
        e.AffectedObjects = CleanList(dto.AffectedObjects);
        e.BuildingElements = CleanList(dto.BuildingElements);
        e.ObservationsInside = CleanList(dto.ObservationsInside);
        e.OutdoorObjects = CleanList(dto.OutdoorObjects);
        e.Infrastructure = CleanList(dto.Infrastructure);
        e.SurfacePhenomena = CleanList(dto.SurfacePhenomena);
        e.CrackWidth = Clean(dto.CrackWidth);
        e.CrackLength = Clean(dto.CrackLength);
        e.OtherObservations = CleanList(dto.OtherObservations);
        e.ContactPhone = Clean(dto.ContactPhone);
        e.ContactEmail = Clean(dto.ContactEmail);
        e.ContactName = Clean(dto.ContactName);
        e.AdditionalInfo = Clean(dto.AdditionalInfo);
    }

    public static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Npgsql принимает для timestamptz только UTC.</summary>
    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static List<string> CleanList(List<string>? list)
        => list?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList() ?? [];
}