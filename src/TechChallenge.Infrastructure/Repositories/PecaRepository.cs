using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public class PecaRepository : BaseRepository<Peca>, IPecaRepository
{
    public PecaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Peca>> ObterAtivosAsync()
        => await _dbSet.Where(p => p.Ativo).ToListAsync();

    public async Task<IEnumerable<Peca>> BuscarPorNomeAsync(string nome)
        => await _dbSet.Where(p => p.Ativo && p.Nome.ToLower().Contains(nome.ToLower())).ToListAsync();
}
