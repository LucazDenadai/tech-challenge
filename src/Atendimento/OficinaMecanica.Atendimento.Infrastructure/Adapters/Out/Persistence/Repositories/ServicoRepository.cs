using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.Repositories;

public class ServicoRepository : BaseRepository<Servico>, IServicoRepository
{
    public ServicoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Servico>> ObterAtivosAsync(CancellationToken ct = default)
        => await _dbSet.Where(s => s.Ativo).ToListAsync(ct);
}
