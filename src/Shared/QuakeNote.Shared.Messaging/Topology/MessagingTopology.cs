using MassTransit;
using QuakeNote.Shared.Contracts.Events;

namespace QuakeNote.Shared.Messaging.Topology;

public static class MessagingTopology
{
    public static void ConfigureEvents(IRabbitMqBusFactoryConfigurator cfg)
    {
        Map<ReportSubmittedEvent>(cfg, Exchanges.Reports, RoutingKeys.ReportSubmitted);
        Map<ReportUpdatedEvent>(cfg, Exchanges.Reports, RoutingKeys.ReportUpdated);
        Map<ReportMatchedEvent>(cfg, Exchanges.Reports, RoutingKeys.ReportMatched);
        Map<EarthquakeCreatedEvent>(cfg, Exchanges.Earthquakes, RoutingKeys.EarthquakeCreated);
        Map<AuthCodeRequestedEvent>(cfg, Exchanges.Auth, RoutingKeys.AuthCodeRequested);
    }

    private static void Map<T>(IRabbitMqBusFactoryConfigurator cfg, string exchange, string routingKey)
        where T : class
    {
        cfg.Message<T>(x => x.SetEntityName(exchange));
        cfg.Publish<T>(x => x.ExchangeType = "topic");
        cfg.Send<T>(x => x.UseRoutingKeyFormatter(_ => routingKey));
    }
}