using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Application.Ports.Out;

public interface IVeiculoRepository : IRepository<Veiculo>
{
    Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task<Veiculo?> ObterPorPlacaAsync(string placa, CancellationToken ct = default);
}
