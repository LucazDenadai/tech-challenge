using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Domain.Interfaces;

public interface IOrdemServicoRepository : IRepository<OrdemServico>
{
    Task<OrdemServico?> ObterComDetalhesAsync(Guid id);
    Task<IEnumerable<OrdemServico>> ObterPorClienteAsync(Guid clienteId);
    Task<IEnumerable<OrdemServico>> ObterPorStatusAsync(StatusOrdemServico status);
    Task<string> GerarNumeroAsync();
}
