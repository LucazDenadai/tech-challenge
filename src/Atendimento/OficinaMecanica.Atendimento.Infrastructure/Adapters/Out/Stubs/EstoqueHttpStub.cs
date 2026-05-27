using Microsoft.Extensions.Logging;
using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Stubs;

public class EstoqueHttpStub : IEstoquePort
{
    private readonly ILogger<EstoqueHttpStub> _logger;

    public EstoqueHttpStub(ILogger<EstoqueHttpStub> logger) => _logger = logger;

    public Task<bool> VerificarDisponibilidadeAsync(IEnumerable<ItemPecaRequest> itens, CancellationToken ct = default)
    {
        _logger.LogInformation("Stub: verificando disponibilidade de {Count} peças — retornando true", itens.Count());
        return Task.FromResult(true);
    }

    public Task<PecaEstoqueDto?> ObterPecaAsync(Guid pecaId, CancellationToken ct = default)
    {
        _logger.LogInformation("Stub: obtendo peça {PecaId} — retornando peça fictícia", pecaId);
        return Task.FromResult<PecaEstoqueDto?>(new(pecaId, "Peça (stub)", "", 0m));
    }
}
