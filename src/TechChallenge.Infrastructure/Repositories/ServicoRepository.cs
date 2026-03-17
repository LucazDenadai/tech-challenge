using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public class ServicoRepository : BaseRepository<Servico>, IServicoRepository
{
    public ServicoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Servico>> ObterAtivosAsync()
        => await _dbSet.Where(s => s.Ativo).ToListAsync();
}
