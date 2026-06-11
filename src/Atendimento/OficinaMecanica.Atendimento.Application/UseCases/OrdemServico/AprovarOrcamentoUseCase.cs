using Microsoft.Extensions.Logging;
using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class AprovarOrcamentoUseCase(IOrdemServicoRepository repository, ILogger<AprovarOrcamentoUseCase> logger)
{
    public async Task ExecutarAsync(Guid osId, bool aprovado, CancellationToken ct = default)
    {
        var os = await repository.ObterPorIdAsync(osId, ct)
            ?? throw new NotFoundException("OrdemServico", osId);

        if (os.Status != StatusOrdemServico.AguardandoAprovacao)
            throw new InvalidOperationException($"OS deve estar em '{StatusOrdemServico.AguardandoAprovacao}' para aprovação. Status atual: '{os.Status}'.");

        os.AlterarStatus(aprovado ? StatusOrdemServico.EmExecucao : StatusOrdemServico.Cancelada);

        await repository.AtualizarAsync(os, ct);
        await repository.SalvarAsync(ct);

        logger.LogInformation("Orcamento processado. OrdemServicoId={OrdemServicoId} Aprovado={Aprovado}",
            osId, aprovado);
    }
}
