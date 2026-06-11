using Microsoft.Extensions.Logging;
using OficinaMecanica.Atendimento.Application.Events;
using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class AtualizarStatusOSUseCase(
    IOrdemServicoRepository repository,
    IEventPublisher eventPublisher,
    IEmailPort emailPort,
    IClienteRepository clienteRepository,
    ILogger<AtualizarStatusOSUseCase> logger)
{
    public async Task ExecutarAsync(Guid osId, StatusOrdemServico novoStatus, CancellationToken ct = default)
    {
        var os = await repository.ObterComDetalhesAsync(osId, ct)
            ?? throw new NotFoundException("OrdemServico", osId);

        var statusAnterior = os.Status;
        os.AlterarStatus(novoStatus);

        await repository.AtualizarAsync(os, ct);
        await repository.SalvarAsync(ct);

        logger.LogInformation("Status de OS alterado. OrdemServicoId={OrdemServicoId} StatusAnterior={StatusAnterior} NovoStatus={NovoStatus}",
            osId, statusAnterior, novoStatus);

        if (novoStatus == StatusOrdemServico.Finalizada)
        {
            var evento = new OsFinalizadaEvent
            {
                OrdemServicoId = os.Id,
                NumeroOS = os.Numero,
                Itens = os.ItensPeca
                    .Select(p => new ItemBaixaDto(p.PecaId, p.Quantidade))
                    .ToList()
            };
            await eventPublisher.PublishOsFinalizadaAsync(evento, ct);

            logger.LogInformation("Evento publicado no RabbitMQ. EventoTipo={EventoTipo} OrdemServicoId={OrdemServicoId}",
                nameof(OsFinalizadaEvent), os.Id);
        }

        var cliente = await clienteRepository.ObterPorIdAsync(os.ClienteId, ct);
        if (cliente is not null)
        {
            try { await emailPort.EnviarAtualizacaoStatusAsync(cliente.Email, os.Numero, novoStatus, ct); }
            catch { /* falha no SMTP não bloqueia a transição de status */ }
        }
    }
}
