using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuakeNote.Earthquakes.Core.Abstractions;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Infrastructure;
using QuakeNote.Earthquakes.Data;
using QuakeNote.Earthquakes.Data.Entities;
using QuakeNote.Shared.Contracts;
using QuakeNote.Shared.Contracts.Errors;
using QuakeNote.Shared.Messaging;

namespace QuakeNote.Earthquakes.Core.Services;

/// <summary>Агрегированная оценка события: приборная часть + макросейсмика по отчётам + статус.</summary>
public class EventAssessmentService(
    EarthquakesDbContext db,
    IReportsGrpcClient reports,
    EarthquakeCache cache,
    IMessagePublisher publisher,
    TimeProvider time,
    ILogger<EventAssessmentService> logger)
{
    /// <summary>Сколько подтверждённых (verified) отчётов нужно, чтобы событие считалось confirmed.</summary>
    private const int ConfirmedMinVerifiedReports = 3;

    /// <summary>Расхождение инструментальной и макросейсмической оценок (в баллах), после которого событие спорное.</summary>
    private const decimal DisputedDifference = 1.5m;

    /// <summary>
    /// Пересчитывает макросейсмическую часть по статистике Reports. Если Reports недоступен, бросает исключение
    /// (обработчик шины повторит попытку).
    /// </summary>
    public async Task RecalculateFromReportsAsync(Guid earthquakeUid, CancellationToken ct = default)
    {
        var assessment = await GetOrCreateAsync(earthquakeUid, ct);
        var stats = await reports.GetStatsAsync(earthquakeUid, ct);
        ApplyReportStats(assessment, stats);
        await SaveAsync(assessment, earthquakeUid, ct);
    }

    /// <summary>
    /// Принимает инструментальную интенсивность от Verification и пересчитывает остальное.
    /// Если Reports недоступен, инструментальная часть всё равно сохраняется.
    /// </summary>
    public async Task<EventAssessmentDto> SetInstrumentalAsync(
        Guid earthquakeUid, decimal? instrumentalIntensity, CancellationToken ct = default)
    {
        var assessment = await GetOrCreateAsync(earthquakeUid, ct);
        assessment.InstrumentalIntensity = instrumentalIntensity;

        try
        {
            var stats = await reports.GetStatsAsync(earthquakeUid, ct);
            ApplyReportStats(assessment, stats);
        }
        catch (RpcException ex)
        {
            logger.LogWarning(ex, "Reports недоступен, макросейсмическая часть события {EarthquakeUid} не обновлена.",
                earthquakeUid);
        }

        await SaveAsync(assessment, earthquakeUid, ct);
        return assessment.ToDto();
    }

    /// <summary>
    /// Админский пересчёт: расстояния до станций, макросейсмика по Reports и повторный запуск приборной ветки
    /// (Verification пересчитает инструментальную интенсивность и подберёт несопоставленные отчёты).
    /// </summary>
    public async Task RequestRecomputeAsync(Guid earthquakeUid, CancellationToken ct = default)
    {
        var earthquake = await db.Earthquakes
            .Include(e => e.Readings).ThenInclude(r => r.Station)
            .FirstOrDefaultAsync(e => e.Uid == earthquakeUid, ct)
            ?? throw new NotFoundException("Событие не найдено.", "earthquake_not_found");

        ReadingDistances.Recalculate(earthquake);
        await db.SaveChangesAsync(ct);
        await cache.InvalidateDetailAsync(earthquakeUid);

        await publisher.PublishBestEffortAsync(logger, earthquake.ToCreatedEvent(), ct);

        try
        {
            await RecalculateFromReportsAsync(earthquakeUid, ct);
        }
        catch (RpcException ex)
        {
            logger.LogWarning(ex, "Reports недоступен, макросейсмика события {EarthquakeUid} не пересчитана.",
                earthquakeUid);
        }
    }

    /// <summary>Статус события: сначала нужны обе оценки; затем сравнение и число подтверждённых отчётов.</summary>
    public static string ComputeStatus(decimal? instrumental, decimal? macroseismic, int verifiedCount)
    {
        if (instrumental is not { } instr || macroseismic is not { } macro)
            return EventVerificationStatuses.InsufficientData;

        if (Math.Abs(instr - macro) > DisputedDifference)
            return EventVerificationStatuses.Disputed;

        return verifiedCount >= ConfirmedMinVerifiedReports
            ? EventVerificationStatuses.Confirmed
            : EventVerificationStatuses.InsufficientData;
    }

    private static void ApplyReportStats(EventAssessment assessment, ReportsStats stats)
    {
        assessment.MacroseismicIntensity = stats.MeanIntensity is { } mean ? Math.Round((decimal)mean, 1) : null;
        assessment.ReportsCount = stats.TotalCount;
        assessment.VerificationStatus = ComputeStatus(
            assessment.InstrumentalIntensity, assessment.MacroseismicIntensity, stats.VerifiedCount);
    }

    private async Task<EventAssessment> GetOrCreateAsync(Guid earthquakeUid, CancellationToken ct)
    {
        var earthquake = await db.Earthquakes.Include(e => e.Assessment)
            .FirstOrDefaultAsync(e => e.Uid == earthquakeUid, ct)
            ?? throw new NotFoundException("Событие не найдено.", "earthquake_not_found");

        return earthquake.Assessment ??= new EventAssessment();
    }

    private async Task SaveAsync(EventAssessment assessment, Guid earthquakeUid, CancellationToken ct)
    {
        assessment.LastRecomputedAt = time.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        await cache.InvalidateDetailAsync(earthquakeUid);
    }
}