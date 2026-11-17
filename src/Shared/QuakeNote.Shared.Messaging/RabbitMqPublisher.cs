using MassTransit;

namespace QuakeNote.Shared.Messaging;

public sealed class RabbitMqPublisher(IPublishEndpoint publishEndpoint) : IMessagePublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        => publishEndpoint.Publish(message, cancellationToken);
}