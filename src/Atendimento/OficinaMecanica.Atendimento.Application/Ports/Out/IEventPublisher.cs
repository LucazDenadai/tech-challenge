using OficinaMecanica.Atendimento.Application.Events;

namespace OficinaMecanica.Atendimento.Application.Ports.Out;

public interface IEventPublisher
{
    Task PublishOsFinalizadaAsync(OsFinalizadaEvent evento, CancellationToken ct = default);
}
