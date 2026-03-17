using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context) : base(context) { }

    public async Task<Cliente?> ObterPorCpfAsync(string cpf)
        => await _dbSet.FirstOrDefaultAsync(c => c.Cpf == cpf);

    public async Task<bool> CpfExisteAsync(string cpf, Guid? excluirId = null)
        => await _dbSet.AnyAsync(c => c.Cpf == cpf && (excluirId == null || c.Id != excluirId));
}
