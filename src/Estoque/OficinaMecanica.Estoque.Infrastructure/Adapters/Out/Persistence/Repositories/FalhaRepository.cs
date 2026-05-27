using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.Out.Persistence.Repositories;

public class FalhaRepository(AppDbContext context) : IFalhaRepository
{
    public async Task AdicionarAsync(FalhaProcessamento falha, CancellationToken ct = default)
    {
        await context.Falhas.AddAsync(falha, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<FalhaProcessamento>> ObterTodosAsync(CancellationToken ct = default)
        => await context.Falhas.OrderByDescending(f => f.OcorridoEm).ToListAsync(ct);

    public async Task<IEnumerable<FalhaProcessamento>> ObterPorOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken ct = default)
        => await context.Falhas.Where(f => f.OrdemServicoId == ordemServicoId).OrderByDescending(f => f.OcorridoEm).ToListAsync(ct);
}
