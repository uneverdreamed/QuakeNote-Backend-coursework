using MassTransit;
using Microsoft.Extensions.Logging;
using QuakeNote.Earthquakes.Core.Services;
using QuakeNote.Shared.Contracts.Errors;
using QuakeNote.Shared.Contracts.Events;

namespace QuakeNote.Earthquakes.Core.Consumers;

/// <summary>
/// Отчёт отредактирован и отвязан от события: пересчитываем прежнее событие,
/// потому что его статистика изменилась. Новая привязка придёт отдельным report.matched.
/// </summary>
public class ReportUpdatedConsumer(
    EventAssessmentService assessments,
    ILogger<ReportUpdatedConsumer> logger) : IConsumer<ReportUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ReportUpdatedEvent> context)
    {
        Guid? previousUid = context.Message.PreviousEarthquakeUid;
        if (previousUid is not { } uid || uid == Guid.Empty)
            return;

        try
        {
            await assessments.RecalculateFromReportsAsync(uid, context.CancellationToken);
        }
        catch (NotFoundException)
        {
            logger.LogWarning("report.updated для неизвестного события {EarthquakeUid}, пропускаем.", uid);
        }
    }
}