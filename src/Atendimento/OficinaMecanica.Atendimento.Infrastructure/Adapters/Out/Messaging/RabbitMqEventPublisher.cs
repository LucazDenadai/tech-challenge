using MassTransit;
using OficinaMecanica.Atendimento.Application.Events;
using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Messaging;

public class RabbitMqEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishOsFinalizadaAsync(OsFinalizadaEvent evento, CancellationToken ct = default)
        => publishEndpoint.Publish(evento, ct);
}
