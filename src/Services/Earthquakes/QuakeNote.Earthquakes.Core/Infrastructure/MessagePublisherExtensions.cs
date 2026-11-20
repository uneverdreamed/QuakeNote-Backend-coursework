using Microsoft.Extensions.Logging;
using QuakeNote.Shared.Messaging;

namespace QuakeNote.Earthquakes.Core.Infrastructure;

internal static class MessagePublisherExtensions
{
    /// <summary>Данные уже сохранены, поэтому недоступность брокера только логируется.</summary>
    public static async Task PublishBestEffortAsync<T>(
        this IMessagePublisher publisher, ILogger logger, T message, CancellationToken ct) where T : class
    {
        try
        {
            await publisher.PublishAsync(message, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Не удалось опубликовать {Event}. Данные сохранены.", typeof(T).Name);
        }
    }
}