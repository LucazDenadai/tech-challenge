using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using OficinaMecanica.Atendimento.Application.Events;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.In.Messaging;

public class BaixaEstoqueConsumerFaultConsumer(
    IFalhaRepository falhaRepository,
    ILogger<BaixaEstoqueConsumerFaultConsumer> logger) : IConsumer<Fault<OsFinalizadaEvent>>
{
    public async Task Consume(ConsumeContext<Fault<OsFinalizadaEvent>> context)
    {
        var evento = context.Message.Message;
        var erro = context.Message.Exceptions.FirstOrDefault()?.Message ?? "Erro desconhecido";
        var tentativas = context.Message.FaultedMessageId is not null ? 1 : 0;

        logger.LogError("Falha ao processar evento. OrdemServicoId={OrdemServicoId} Motivo={Motivo}",
            evento.OrdemServicoId, erro);

        logger.LogWarning("Falha de processamento registrada. OrdemServicoId={OrdemServicoId} TentativaNumero={TentativaNumero}",
            evento.OrdemServicoId, tentativas);

        var payloadJson = JsonSerializer.Serialize(evento);
        var falha = new FalhaProcessamento(evento.EventId, evento.OrdemServicoId, erro, payloadJson);
        await falhaRepository.AdicionarAsync(falha, context.CancellationToken);
    }
}
