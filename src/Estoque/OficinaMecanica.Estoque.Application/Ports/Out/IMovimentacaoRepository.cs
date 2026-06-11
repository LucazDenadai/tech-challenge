using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Application.Ports.Out;

public interface IMovimentacaoRepository
{
    Task AdicionarAsync(MovimentacaoEstoque movimentacao, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
    Task<bool> ExisteMovimentacaoPorOsIdAsync(Guid osId, CancellationToken ct = default);
}
