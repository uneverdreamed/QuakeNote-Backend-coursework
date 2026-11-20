using MassTransit;
using Microsoft.Extensions.Logging;
using QuakeNote.Earthquakes.Core.Services;
using QuakeNote.Shared.Contracts.Errors;
using QuakeNote.Shared.Contracts.Events;

namespace QuakeNote.Earthquakes.Core.Consumers;

/// <summary>Отчёт привязан к событию: пересчитываем макросейсмическую часть оценки этого события.</summary>
public class ReportMatchedConsumer(
    EventAssessmentService assessments,
    ILogger<ReportMatchedConsumer> logger) : IConsumer<ReportMatchedEvent>
{
    public async Task Consume(ConsumeContext<ReportMatchedEvent> context)
    {
        Guid? earthquakeUid = context.Message.EarthquakeUid;
        if (earthquakeUid is not { } uid || uid == Guid.Empty)
            return;

        try
        {
            await assessments.RecalculateFromReportsAsync(uid, context.CancellationToken);
        }
        catch (NotFoundException)
        {
            // событие успели удалить: пересчитывать нечего, повторять бессмысленно
            logger.LogWarning("report.matched для неизвестного события {EarthquakeUid}, пропускаем.", uid);
        }
        // остальные ошибки (например, Reports недоступен) уходят в повтор MassTransit
    }
}