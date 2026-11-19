using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuakeNote.Reports.Core.Abstractions;
using QuakeNote.Reports.Core.Dtos;
using QuakeNote.Reports.Core.Infrastructure;
using QuakeNote.Reports.Data;
using QuakeNote.Reports.Data.Entities;
using QuakeNote.Shared.Contracts.Dtos;
using QuakeNote.Shared.Contracts.Errors;
using QuakeNote.Shared.Messaging;

namespace QuakeNote.Reports.Core.Services;

public class ReportService(
    ReportsDbContext db,
    IEditWindowPolicy editWindow,
    ReportStatsService stats,
    IMessagePublisher publisher,
    TimeProvider time,
    ILogger<ReportService> logger)
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private const int MaxPublicReports = 1000;
    private const int MaxUnmatchedReports = 1000;
    private static readonly TimeSpan MaxUnmatchedSpan = TimeSpan.FromDays(7);

    /// <summary>Создать отчёт (userUid = null для анонимного) и поставить его в очередь на верификацию.</summary>
    public async Task<ReportDto> CreateAsync(CreateReportDto dto, Guid? userUid, CancellationToken ct = default)
    {
        var report = new Report
        {
            UserUid = userUid,
            Latitude = dto.Latitude!.Value,
            Longitude = dto.Longitude!.Value,
            FeltIntensity = (short)dto.FeltIntensity!.Value,
            RegionName = ReportMappings.Clean(dto.RegionName),
            CreatedAt = time.GetUtcNow().UtcDateTime
        };

        db.Reports.Add(report);
        await db.SaveChangesAsync(ct);

        await publisher.PublishBestEffortAsync(logger, report.ToSubmittedEvent(), ct);
        return report.ToDto();
    }

    /// <summary>Изменить отчёт (только владелец, пока открыто окно редактирования).</summary>
    public async Task<ReportDto> UpdateAsync(Guid uid, UpdateReportDto dto, Guid userUid, CancellationToken ct = default)
    {
        var report = await ReportGuards.GetReportAsync(db, uid, ct);
        ReportGuards.EnsureOwner(report, userUid);
        ReportGuards.EnsureEditable(report, editWindow);

        report.Latitude = dto.Latitude!.Value;
        report.Longitude = dto.Longitude!.Value;
        report.FeltIntensity = (short)dto.FeltIntensity!.Value;
        report.RegionName = ReportMappings.Clean(dto.RegionName);
        var previousEarthquakeUid = report.ResetVerification(time.GetUtcNow().UtcDateTime);

        await db.SaveChangesAsync(ct);

        await stats.InvalidateAsync(previousEarthquakeUid);
        await publisher.PublishBestEffortAsync(logger, report.ToUpdatedEvent(previousEarthquakeUid), ct);
        return report.ToDto();
    }

    /// <summary>История отчётов пользователя, новые сверху.</summary>
    public async Task<PagedResultDto<ReportDto>> GetMineAsync(
        Guid userUid, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = NormalizePaging(page, pageSize);

        var query = db.Reports.AsNoTracking().Where(r => r.UserUid == userUid);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(ReportMappings.ToDtoExpression)
            .ToListAsync(ct);

        return new PagedResultDto<ReportDto>(items, page, pageSize, total);
    }

    /// <summary>Один отчёт с анкетой: владелец или администратор.</summary>
    public async Task<ReportWithDetailsDto> GetWithDetailsAsync(
        Guid uid, Guid requesterUid, bool isAdmin, CancellationToken ct = default)
    {
        var report = await ReportGuards.GetReportAsync(db, uid, ct);
        if (!isAdmin) ReportGuards.EnsureOwner(report, requesterUid);
        return report.ToDtoWithDetails();
    }

    /// <summary>
    /// Публичная выдача по событию. Reports не знает, существует ли событие (оно в другой БД),
    /// поэтому для неизвестного uid возвращается пустой список.
    /// </summary>
    public async Task<List<AnonymizedReportDto>> GetByEarthquakeAsync(Guid earthquakeUid, CancellationToken ct = default)
        => await db.Reports.AsNoTracking()
            .Where(r => r.EarthquakeUid == earthquakeUid)
            .OrderBy(r => r.CreatedAt).ThenBy(r => r.Id)
            .Take(MaxPublicReports)
            .Select(ReportMappings.ToAnonymizedExpression)
            .ToListAsync(ct);

    /// <summary>Internal: применить результат верификации.</summary>
    public async Task<ReportDto> ApplyMatchAsync(Guid uid, MatchReportDto dto, CancellationToken ct = default)
    {
        var report = await ReportGuards.GetReportAsync(db, uid, ct);
        var previousEarthquakeUid = report.EarthquakeUid;

        report.ValidationStatus = dto.ValidationStatus;
        report.ExpectedIntensity = dto.ExpectedIntensity;
        report.Deviation = dto.Deviation;
        report.EarthquakeUid = dto.MatchedEarthquakeUid;
        report.MatchedAt = time.GetUtcNow().UtcDateTime;

        await db.SaveChangesAsync(ct);

        await stats.InvalidateAsync(previousEarthquakeUid, dto.MatchedEarthquakeUid);
        return report.ToDto();
    }

    /// <summary>Internal: отчёты без привязки к событию за период (для обратного сопоставления).</summary>
    public async Task<List<UnmatchedReportDto>> GetUnmatchedAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        from = ReportMappings.ToUtc(from);
        to = ReportMappings.ToUtc(to);

        if (from >= to)
            throw new BadRequestException("Начало периода должно быть раньше конца.", "invalid_period");
        if (to - from > MaxUnmatchedSpan)
            throw new BadRequestException("Период не может быть больше 7 суток.", "period_too_long");

        return await db.Reports.AsNoTracking()
            .Where(r => r.EarthquakeUid == null && r.CreatedAt >= from && r.CreatedAt <= to)
            .OrderBy(r => r.CreatedAt)
            .Take(MaxUnmatchedReports)
            .Select(r => new UnmatchedReportDto(r.Uid, r.UserUid, r.Latitude, r.Longitude, r.FeltIntensity, r.CreatedAt))
            .ToListAsync(ct);
    }

    private static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
        => (Math.Max(page, 1), pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize));
}