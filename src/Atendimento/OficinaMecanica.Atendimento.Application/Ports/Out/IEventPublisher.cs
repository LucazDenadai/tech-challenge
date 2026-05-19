namespace OficinaMecanica.Atendimento.Application.Ports.Out;

// TODO: implementado no CARD-08 (RabbitMQ + MassTransit)
public interface IEventPublisher
{
    Task PublishAsync<T>(string routingKey, T payload, CancellationToken ct = default);
}
