using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public class OrdemServicoRepository : BaseRepository<OrdemServico>, IOrdemServicoRepository
{
    public OrdemServicoRepository(AppDbContext context) : base(context) { }

    public async Task<OrdemServico?> ObterComDetalhesAsync(Guid id)
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<OrdemServico>> ObterPorClienteAsync(Guid clienteId)
        => await _dbSet.Where(o => o.ClienteId == clienteId).ToListAsync();

    public async Task<IEnumerable<OrdemServico>> ObterPorStatusAsync(StatusOrdemServico status)
        => await _dbSet.Where(o => o.Status == status).ToListAsync();

    public async Task<string> GerarNumeroAsync()
    {
        var ano = DateTime.UtcNow.Year;
        var count = await _dbSet.CountAsync(o => o.DataAbertura.Year == ano);
        return $"OS-{ano}-{(count + 1):D4}";
    }
}
