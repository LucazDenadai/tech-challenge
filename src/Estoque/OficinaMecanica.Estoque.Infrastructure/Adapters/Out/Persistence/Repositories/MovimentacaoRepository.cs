using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.Out.Persistence.Repositories;

public class MovimentacaoRepository : IMovimentacaoRepository
{
    private readonly AppDbContext _context;

    public MovimentacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(MovimentacaoEstoque movimentacao, CancellationToken ct = default)
        => await _context.Movimentacoes.AddAsync(movimentacao, ct);

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task<bool> ExisteMovimentacaoPorOsIdAsync(Guid osId, CancellationToken ct = default)
        => await _context.Movimentacoes.AnyAsync(m => m.OsId == osId, ct);
}
