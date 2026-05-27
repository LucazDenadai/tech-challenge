using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Application.Ports.Out;

public interface IPecaRepository : IRepository<Peca>
{
    Task<IEnumerable<Peca>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
