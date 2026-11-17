using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuakeNote.Shared.Messaging.Topology;

namespace QuakeNote.Shared.Messaging.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddQuakeNoteMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null,
        Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext>? configureEndpoints = null)
    {
        var section = configuration.GetSection("RabbitMq");
        var host = section["Host"] ?? "localhost";
        var port = ushort.Parse(section["Port"] ?? "5672");
        var virtualHost = section["VirtualHost"] ?? "/";
        var username = section["Username"]
            ?? throw new InvalidOperationException("RabbitMq:Username is not configured.");
        var password = section["Password"]
            ?? throw new InvalidOperationException("RabbitMq:Password is not configured.");

        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, port, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                MessagingTopology.ConfigureEvents(cfg);
                configureEndpoints?.Invoke(cfg, context);
            });
        });

        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
        return services;
    }

    /// <summary>
    /// Создаёт очередь для консьюмера и привязывает её к topic-exchange по routing key.
    /// </summary>
    public static void AddTopicConsumer<TConsumer>(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context,
        string queueName,
        string exchange,
        string routingKey)
        where TConsumer : class, IConsumer
    {
        cfg.ReceiveEndpoint(queueName, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Bind(exchange, b =>
            {
                b.ExchangeType = "topic";
                b.RoutingKey = routingKey;
            });
            e.ConfigureConsumer<TConsumer>(context);
        });
    }
}