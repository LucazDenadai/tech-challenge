using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.Out.Persistence.Repositories;

public class PecaRepository : BaseRepository<Peca>, IPecaRepository
{
    public PecaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Peca>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        => await _dbSet.Where(p => ids.Contains(p.Id)).ToListAsync(ct);
}
