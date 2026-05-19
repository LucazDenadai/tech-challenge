using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.Repositories;

public class PecaRepository : BaseRepository<Peca>, IPecaRepository
{
    public PecaRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ExisteAsync(Guid pecaId, CancellationToken ct = default)
        => await _dbSet.AnyAsync(p => p.Id == pecaId && p.Ativo, ct);

    public async Task<string?> ObterNomeAsync(Guid pecaId, CancellationToken ct = default)
        => await _dbSet.Where(p => p.Id == pecaId).Select(p => p.Nome).FirstOrDefaultAsync(ct);
}
