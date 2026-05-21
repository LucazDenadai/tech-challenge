using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Application.Ports.Out;

public interface IPecaRepository : IRepository<Peca>
{
    Task<bool> ExisteAsync(Guid pecaId, CancellationToken ct = default);
    Task<string?> ObterNomeAsync(Guid pecaId, CancellationToken ct = default);
    Task<IEnumerable<Peca>> BuscarPorNomeAsync(string nome, CancellationToken ct = default);
}
