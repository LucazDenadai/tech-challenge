using TechChallenge.Domain.Entities;

namespace TechChallenge.Domain.Interfaces;

public interface IVeiculoRepository : IRepository<Veiculo>
{
    Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId);
    Task<Veiculo?> ObterPorPlacaAsync(string placa);
    Task<IEnumerable<Veiculo>> ObterPorDocumentoClienteAsync(string documento);
}
