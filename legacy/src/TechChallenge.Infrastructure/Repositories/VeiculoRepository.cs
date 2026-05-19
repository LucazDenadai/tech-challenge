using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public class VeiculoRepository : BaseRepository<Veiculo>, IVeiculoRepository
{
    public VeiculoRepository(AppDbContext context) : base(context) { }

    public override async Task<IEnumerable<Veiculo>> ObterTodosAsync()
        => await _dbSet.Include(v => v.Cliente).ToListAsync();

    public override async Task<Veiculo?> ObterPorIdAsync(Guid id)
        => await _dbSet.Include(v => v.Cliente).FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId)
        => await _dbSet.Include(v => v.Cliente).Where(v => v.ClienteId == clienteId).ToListAsync();

    [ExcludeFromCodeCoverage]
    public async Task<Veiculo?> ObterPorPlacaAsync(string placa)
        => await _dbSet.FirstOrDefaultAsync(v => v.Placa == placa);

    public override async Task<int> SalvarAsync()
    {
        try { return await base.SalvarAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23503")
        {
            throw new InvalidOperationException("Não é possível remover o veículo pois existem ordens de serviço associadas.");
        }
    }

    public async Task<IEnumerable<Veiculo>> ObterPorDocumentoClienteAsync(string documento)
    {
        var docNorm = documento.Replace(".", "").Replace("-", "").Replace("/", "").Trim();
        return await _dbSet
            .Include(v => v.Cliente)
            .Where(v => v.Cliente != null &&
                (v.Cliente.Documento == documento || v.Cliente.Documento == docNorm))
            .ToListAsync();
    }
}
