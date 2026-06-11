using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Application.Ports.Out;

public interface IFalhaRepository
{
    Task AdicionarAsync(FalhaProcessamento falha, CancellationToken ct = default);
    Task<IEnumerable<FalhaProcessamento>> ObterTodosAsync(CancellationToken ct = default);
    Task<IEnumerable<FalhaProcessamento>> ObterPorOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken ct = default);
}
