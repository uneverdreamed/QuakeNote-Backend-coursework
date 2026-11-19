using Microsoft.Extensions.Logging;
using QuakeNote.Shared.Messaging;

namespace QuakeNote.Reports.Core.Infrastructure;

internal static class MessagePublisherExtensions
{
    /// <summary>
    /// Отчёт уже сохранён в БД, поэтому недоступность брокера не должна превращать ответ в ошибку:
    /// пользователь получит 201, а верификация подхватит отчёт позже (по earthquake.created).
    /// </summary>
    public static async Task PublishBestEffortAsync<T>(
        this IMessagePublisher publisher, ILogger logger, T message, CancellationToken ct) where T : class
    {
        try
        {
            await publisher.PublishAsync(message, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось опубликовать {Event}. Данные сохранены, верификация будет отложена.",
                typeof(T).Name);
        }
    }
}