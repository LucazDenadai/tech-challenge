using System.Text.Json;
using MassTransit;
using OficinaMecanica.Atendimento.Application.Events;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.In.Messaging;

public class BaixaEstoqueConsumerFaultConsumer(IFalhaRepository falhaRepository) : IConsumer<Fault<OsFinalizadaEvent>>
{
    public async Task Consume(ConsumeContext<Fault<OsFinalizadaEvent>> context)
    {
        var evento = context.Message.Message;
        var erro = context.Message.Exceptions.FirstOrDefault()?.Message ?? "Erro desconhecido";
        var payloadJson = JsonSerializer.Serialize(evento);

        var falha = new FalhaProcessamento(evento.EventId, evento.OrdemServicoId, erro, payloadJson);
        await falhaRepository.AdicionarAsync(falha, context.CancellationToken);
    }
}
