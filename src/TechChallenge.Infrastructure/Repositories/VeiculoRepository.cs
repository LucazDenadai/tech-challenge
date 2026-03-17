using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public class VeiculoRepository : BaseRepository<Veiculo>, IVeiculoRepository
{
    public VeiculoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId)
        => await _dbSet.Include(v => v.Cliente).Where(v => v.ClienteId == clienteId).ToListAsync();

    public async Task<Veiculo?> ObterPorPlacaAsync(string placa)
        => await _dbSet.FirstOrDefaultAsync(v => v.Placa == placa);
}
