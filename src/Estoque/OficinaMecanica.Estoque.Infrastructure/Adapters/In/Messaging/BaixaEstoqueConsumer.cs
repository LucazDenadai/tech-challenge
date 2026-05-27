using MassTransit;
using OficinaMecanica.Estoque.Application.Events;
using OficinaMecanica.Estoque.Application.UseCases;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.In.Messaging;

public class BaixaEstoqueConsumer(BaixarEstoqueUseCase useCase) : IConsumer<OsFinalizadaEvent>
{
    public async Task Consume(ConsumeContext<OsFinalizadaEvent> context)
    {
        var evento = context.Message;
        var itens = evento.Itens.Select(i => new ItemBaixa(i.PecaId, i.Quantidade));
        await useCase.ExecutarAsync(evento.OrdemServicoId, itens, context.CancellationToken);
    }
}
