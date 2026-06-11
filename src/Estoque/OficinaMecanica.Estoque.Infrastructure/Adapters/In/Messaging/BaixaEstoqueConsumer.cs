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
        logger.LogInformation("BaixaEstoqueConsumer: OsId={OsId} Itens={Count}", evento.OrdemServicoId, evento.Itens.Count);
        var itens = evento.Itens.Select(i => new ItemBaixa(i.PecaId, i.Quantidade));
        await useCase.ExecutarAsync(evento.OrdemServicoId, itens, context.CancellationToken);
        logger.LogInformation("BaixaEstoqueConsumer: baixa concluida OsId={OsId}", evento.OrdemServicoId);
    }
}
