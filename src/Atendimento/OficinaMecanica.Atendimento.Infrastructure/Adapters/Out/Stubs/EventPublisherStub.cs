using Microsoft.Extensions.Logging;
using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Stubs;

// Substituído por RabbitMQ no CARD-08
public class EventPublisherStub : IEventPublisher
{
    private readonly ILogger<EventPublisherStub> _logger;

    public EventPublisherStub(ILogger<EventPublisherStub> logger) => _logger = logger;

    public Task PublishAsync<T>(string routingKey, T payload, CancellationToken ct = default)
    {
        _logger.LogInformation("Evento publicado: {RoutingKey} | payload: {Payload}", routingKey, payload);
        return Task.CompletedTask;
    }
}
