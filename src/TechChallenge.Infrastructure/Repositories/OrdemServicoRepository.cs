using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public class OrdemServicoRepository : BaseRepository<OrdemServico>, IOrdemServicoRepository
{
    public OrdemServicoRepository(AppDbContext context) : base(context) { }

    public override Task AtualizarAsync(OrdemServico entidade)
    {
        foreach (var historico in entidade.Historico)
        {
            if (_context.Entry(historico).State == EntityState.Detached)
            {
                _context.Set<HistoricoStatusOS>().Add(historico);
            }
        }

        return base.AtualizarAsync(entidade);
    }

    public override async Task<IEnumerable<OrdemServico>> ObterTodosAsync()
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Include(o => o.Historico)
            .ToListAsync();

    public async Task<OrdemServico?> ObterComDetalhesAsync(Guid id)
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Include(o => o.Historico)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<OrdemServico>> ObterPorClienteAsync(Guid clienteId)
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Where(o => o.ClienteId == clienteId)
            .ToListAsync();

    public async Task<IEnumerable<OrdemServico>> ObterPorStatusAsync(StatusOrdemServico status)
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Where(o => o.Status == status)
            .ToListAsync();

    public async Task<IEnumerable<OrdemServico>> FiltrarAsync(Guid? clienteId, StatusOrdemServico? status)
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Where(o => (clienteId == null || o.ClienteId == clienteId)
                     && (status == null || o.Status == status))
            .ToListAsync();

    public async Task<(double TempoMedioHoras, int Total)> ObterTempoMedioExecucaoAsync()
    {
        var ordensFinalizadas = await _dbSet
            .Where(o => (o.Status == StatusOrdemServico.Finalizada || o.Status == StatusOrdemServico.Entregue)
                     && o.DataFechamento != null)
            .Select(o => new { o.DataAbertura, DataFechamento = o.DataFechamento!.Value })
            .ToListAsync();

        if (ordensFinalizadas.Count == 0)
            return (0, 0);

        var tempoMedio = ordensFinalizadas.Average(o => (o.DataFechamento - o.DataAbertura).TotalHours);
        return (Math.Round(tempoMedio, 2), ordensFinalizadas.Count);
    }

    public async Task<string> GerarNumeroAsync()
    {
        var ano = DateTime.UtcNow.Year;
        var prefix = $"OS-{ano}-";

        var ultimo = await _dbSet
            .Where(o => o.Numero.StartsWith(prefix))
            .Select(o => o.Numero)
            .ToListAsync();

        var sequencia = ultimo
            .Select(n => int.TryParse(n[prefix.Length..], out var seq) ? seq : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{(sequencia + 1):D4}";
    }
}
