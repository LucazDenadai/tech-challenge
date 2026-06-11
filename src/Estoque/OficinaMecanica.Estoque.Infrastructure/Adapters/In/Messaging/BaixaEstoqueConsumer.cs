using MassTransit;
using Microsoft.Extensions.Logging;
using OficinaMecanica.Atendimento.Application.Events;
using OficinaMecanica.Estoque.Application.UseCases;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.In.Messaging;

public class BaixaEstoqueConsumer(BaixarEstoqueUseCase useCase, ILogger<BaixaEstoqueConsumer> logger) : IConsumer<OsFinalizadaEvent>
{
    public async Task Consume(ConsumeContext<OsFinalizadaEvent> context)
    {
        var evento = context.Message;
        logger.LogInformation("Evento consumido do RabbitMQ. EventoTipo={EventoTipo} OrdemServicoId={OrdemServicoId}",
            nameof(OsFinalizadaEvent), evento.OrdemServicoId);

        var itens = evento.Itens.Select(i => new ItemBaixa(i.PecaId, i.Quantidade));
        await useCase.ExecutarAsync(evento.OrdemServicoId, itens, context.CancellationToken);
    }
}
