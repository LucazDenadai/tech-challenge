using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context) { }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> EmailExisteAsync(string email, Guid? excluirId = null)
        => await _dbSet.AnyAsync(u => u.Email == email && (excluirId == null || u.Id != excluirId));

    public async Task<IEnumerable<Usuario>> BuscarPorEmailAsync(string email)
        => await _dbSet.Where(u => u.Ativo && u.Email.ToLower().Contains(email.ToLower())).ToListAsync();
}
