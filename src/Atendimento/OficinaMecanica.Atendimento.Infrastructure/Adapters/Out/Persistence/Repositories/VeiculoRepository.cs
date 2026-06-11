using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Entities;
using OrdemServico = OficinaMecanica.Atendimento.Domain.Entities.OrdemServico;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.Repositories;

public class VeiculoRepository : BaseRepository<Veiculo>, IVeiculoRepository
{
    public VeiculoRepository(AppDbContext context) : base(context) { }

    public override async Task<IEnumerable<Veiculo>> ObterTodosAsync(CancellationToken ct = default)
        => await _dbSet.Include(v => v.Cliente).ToListAsync(ct);

    public override async Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.Include(v => v.Cliente).FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId, CancellationToken ct = default)
        => await _dbSet.Include(v => v.Cliente).Where(v => v.ClienteId == clienteId).ToListAsync(ct);

    public async Task<IEnumerable<Veiculo>> ObterPorDocumentoClienteAsync(string documento, CancellationToken ct = default)
        => await _dbSet.Include(v => v.Cliente)
            .Where(v => v.Cliente != null && v.Cliente.Documento == documento)
            .ToListAsync(ct);

    public async Task<bool> PossuiOrdensServicoAsync(Guid veiculoId, CancellationToken ct = default)
        => await _context.Set<OrdemServico>().AnyAsync(os => os.VeiculoId == veiculoId, ct);

    [ExcludeFromCodeCoverage]
    public async Task<Veiculo?> ObterPorPlacaAsync(string placa, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(v => v.Placa == placa, ct);

    public override async Task<int> SalvarAsync(CancellationToken ct = default)
    {
        try { return await base.SalvarAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23503")
        {
            throw new InvalidOperationException("Não é possível remover o veículo pois existem ordens de serviço associadas.");
        }
    }
}
