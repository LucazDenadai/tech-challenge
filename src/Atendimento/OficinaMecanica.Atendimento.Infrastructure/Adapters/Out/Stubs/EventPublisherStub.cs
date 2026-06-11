using Microsoft.Extensions.Logging;
using OficinaMecanica.Atendimento.Application.Events;
using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Stubs;

public class EventPublisherStub(ILogger<EventPublisherStub> logger) : IEventPublisher
{
    public Task PublishOsFinalizadaAsync(OsFinalizadaEvent evento, CancellationToken ct = default)
    {
        logger.LogInformation("Evento publicado (stub): OsId={OsId} Itens={Count}",
            evento.OrdemServicoId, evento.Itens.Count);
        return Task.CompletedTask;
    }
}
