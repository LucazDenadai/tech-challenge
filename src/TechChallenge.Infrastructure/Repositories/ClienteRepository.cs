using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context) : base(context) { }

    [ExcludeFromCodeCoverage]
    public async Task<Cliente?> ObterPorDocumentoAsync(string documento)
        => await _dbSet.FirstOrDefaultAsync(c => c.Documento == documento);

    public async Task<bool> DocumentoExisteAsync(string documento, Guid? excluirId = null)
        => await _dbSet.AnyAsync(c => c.Documento == documento && (excluirId == null || c.Id != excluirId));

    public async Task<IEnumerable<Cliente>> BuscarAsync(string termo)
        => await _dbSet.Where(c =>
            c.Nome.ToLower().Contains(termo.ToLower()) ||
            c.Documento.Contains(termo) ||
            c.Email.ToLower().Contains(termo.ToLower()))
        .ToListAsync();

    public async Task<IEnumerable<(Cliente Cliente, int TotalOrdens)>> ObterTodosComDetalhesAsync()
        => await _dbSet
            .Include(c => c.Veiculos)
            .Select(c => new ValueTuple<Cliente, int>(c, _context.Set<OrdemServico>().Count(o => o.ClienteId == c.Id)))
            .ToListAsync();

    public async Task<IEnumerable<(Cliente Cliente, int TotalOrdens)>> BuscarComDetalhesAsync(string termo)
        => await _dbSet
            .Include(c => c.Veiculos)
            .Where(c =>
                c.Nome.ToLower().Contains(termo.ToLower()) ||
                c.Documento.Contains(termo) ||
                c.Email.ToLower().Contains(termo.ToLower()))
            .Select(c => new ValueTuple<Cliente, int>(c, _context.Set<OrdemServico>().Count(o => o.ClienteId == c.Id)))
            .ToListAsync();
}
