using Microsoft.Extensions.Logging;
using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Stubs;

// Substituído por HTTP client com Polly no CARD-09
public class EstoqueHttpStub : IEstoquePort
{
    private readonly ILogger<EstoqueHttpStub> _logger;

    public EstoqueHttpStub(ILogger<EstoqueHttpStub> logger) => _logger = logger;

    public Task<bool> VerificarDisponibilidadeAsync(IEnumerable<ItemPecaRequest> itens, CancellationToken ct = default)
    {
        _logger.LogInformation("Stub: verificando disponibilidade de {Count} peças — retornando true", itens.Count());
        return Task.FromResult(true);
    }
}
