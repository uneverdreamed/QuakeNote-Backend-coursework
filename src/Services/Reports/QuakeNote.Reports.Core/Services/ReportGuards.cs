using Microsoft.EntityFrameworkCore;
using QuakeNote.Reports.Core.Abstractions;
using QuakeNote.Reports.Data;
using QuakeNote.Reports.Data.Entities;
using QuakeNote.Shared.Contracts;
using QuakeNote.Shared.Contracts.Errors;

namespace QuakeNote.Reports.Core.Services;

internal static class ReportGuards
{
    public static async Task<Report> GetReportAsync(ReportsDbContext db, Guid uid, CancellationToken ct)
        => await db.Reports.Include(r => r.Detail).FirstOrDefaultAsync(r => r.Uid == uid, ct)
           ?? throw new NotFoundException("Отчёт не найден.", "report_not_found");

    /// <summary>Анонимный отчёт (UserUid = null) не принадлежит никому, редактировать его нельзя.</summary>
    public static void EnsureOwner(Report report, Guid userUid)
    {
        if (report.UserUid != userUid)
            throw new ForbiddenException("Отчёт принадлежит другому пользователю.", "not_report_owner");
    }

    public static void EnsureEditable(Report report, IEditWindowPolicy window)
    {
        if (!window.IsOpen(report.CreatedAt))
            throw new ConflictException(
                $"Время редактирования отчёта ({window.Window.TotalMinutes:0} мин) истекло.", "edit_window_expired");
    }

    /// <summary>
    /// Результат прежней верификации после правки недействителен: сбрасываем его и отвязываем от события.
    /// Возвращает событие, к которому отчёт был привязан до правки.
    /// </summary>
    public static Guid? ResetVerification(this Report report, DateTime now)
    {
        var previousEarthquakeUid = report.EarthquakeUid;
        report.ValidationStatus = ValidationStatuses.Pending;
        report.ExpectedIntensity = null;
        report.Deviation = null;
        report.MatchedAt = null;
        report.EarthquakeUid = null;
        report.UpdatedAt = now;
        return previousEarthquakeUid;
    }
}