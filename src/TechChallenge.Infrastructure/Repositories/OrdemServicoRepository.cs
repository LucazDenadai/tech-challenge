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
            .OrderByDescending(o => o.DataAbertura)
            .ToListAsync();

    public async Task<OrdemServico?> ObterComDetalhesAsync(Guid id)
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Include(o => o.Historico)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<OrdemServico?> ObterPorNumeroAsync(string numero)
        => await _dbSet
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Include(o => o.Historico)
            .FirstOrDefaultAsync(o => o.Numero == numero);

    public async Task<OrdemServico?> ObterComDetalhesPorNumeroAsync(string numero)
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Include(o => o.Historico)
            .FirstOrDefaultAsync(o => o.Numero == numero);

    public async Task<IEnumerable<OrdemServico>> ObterPorClienteAsync(Guid clienteId)
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Where(o => o.ClienteId == clienteId)
            .OrderByDescending(o => o.DataAbertura)
            .ToListAsync();

    public async Task<IEnumerable<OrdemServico>> ObterPorStatusAsync(StatusOrdemServico status)
        => await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.DataAbertura)
            .ToListAsync();

    public async Task<IEnumerable<OrdemServico>> FiltrarAsync(string? busca, StatusOrdemServico? status)
    {
        var query = _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Veiculo)
            .Include(o => o.ItensServico).ThenInclude(i => i.Servico)
            .Include(o => o.ItensPeca).ThenInclude(i => i.Peca)
            .Include(o => o.Historico)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaUpper = busca.ToUpperInvariant();
            query = query.Where(o =>
                o.Numero.ToUpper().Contains(buscaUpper) ||
                (o.Cliente != null && o.Cliente.Documento.Contains(busca)) ||
                (o.Veiculo != null && o.Veiculo.Placa.ToUpper().Contains(buscaUpper)));
        }

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        return await query.OrderByDescending(o => o.DataAbertura).ToListAsync();
    }

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
