using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class AtualizarStatusOSUseCase(
    IOrdemServicoRepository repository,
    IEventPublisher eventPublisher,
    IEmailPort emailPort,
    IClienteRepository clienteRepository)
{
    public async Task ExecutarAsync(Guid osId, StatusOrdemServico novoStatus, CancellationToken ct = default)
    {
        var os = await repository.ObterComDetalhesAsync(osId, ct)
            ?? throw new NotFoundException("OrdemServico", osId);

        os.AlterarStatus(novoStatus);

        await repository.AtualizarAsync(os, ct);
        await repository.SalvarAsync(ct);

        if (novoStatus == StatusOrdemServico.Finalizada)
            await eventPublisher.PublishAsync("os.finalizada", new { osId = os.Id, numero = os.Numero }, ct);

        var cliente = await clienteRepository.ObterPorIdAsync(os.ClienteId, ct);
        if (cliente is not null)
            await emailPort.EnviarAtualizacaoStatusAsync(cliente.Email, os.Numero, novoStatus, ct);
    }
}
