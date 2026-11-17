namespace QuakeNote.Shared.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}