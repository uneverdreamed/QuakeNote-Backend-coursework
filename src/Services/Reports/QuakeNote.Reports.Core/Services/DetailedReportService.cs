using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuakeNote.Reports.Core.Abstractions;
using QuakeNote.Reports.Core.Dtos;
using QuakeNote.Reports.Core.Infrastructure;
using QuakeNote.Reports.Data;
using QuakeNote.Reports.Data.Entities;
using QuakeNote.Shared.Contracts.Errors;
using QuakeNote.Shared.Messaging;

namespace QuakeNote.Reports.Core.Services;

public class DetailedReportService(
    ReportsDbContext db,
    IEditWindowPolicy editWindow,
    ReportStatsService stats,
    IMessagePublisher publisher,
    TimeProvider time,
    ILogger<DetailedReportService> logger)
{
    /// <summary>Приложить анкету к отчёту (один раз). Окно 15 минут здесь не действует.</summary>
    public async Task<DetailedReportDto> CreateAsync(
        Guid reportUid, DetailedReportDto dto, Guid userUid, CancellationToken ct = default)
    {
        var report = await ReportGuards.GetReportAsync(db, reportUid, ct);
        ReportGuards.EnsureOwner(report, userUid);

        if (report.Detail is not null)
            throw new ConflictException(
                "Расширенная анкета уже приложена. Для изменения используйте PUT.", "details_already_exist");

        var detail = new DetailedReport();
        dto.ApplyTo(detail);
        report.Detail = detail;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // два одновременных POST: уникальный индекс по report_id пропустит только первый
            throw new ConflictException(
                "Расширенная анкета уже приложена. Для изменения используйте PUT.", "details_already_exist");
        }

        return detail.ToDto();
    }

    /// <summary>Изменить анкету (владелец, окно редактирования). Сбрасывает верификацию отчёта.</summary>
    public async Task<DetailedReportDto> UpdateAsync(
        Guid reportUid, DetailedReportDto dto, Guid userUid, CancellationToken ct = default)
    {
        var report = await ReportGuards.GetReportAsync(db, reportUid, ct);
        ReportGuards.EnsureOwner(report, userUid);

        var detail = report.Detail
            ?? throw new NotFoundException("У отчёта нет расширенной анкеты.", "details_not_found");

        ReportGuards.EnsureEditable(report, editWindow);

        dto.ApplyTo(detail);
        var previousEarthquakeUid = report.ResetVerification(time.GetUtcNow().UtcDateTime);

        await db.SaveChangesAsync(ct);

        await stats.InvalidateAsync(previousEarthquakeUid);
        await publisher.PublishBestEffortAsync(logger, report.ToUpdatedEvent(previousEarthquakeUid), ct);
        return detail.ToDto();
    }
}