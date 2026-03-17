using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure.Data;

namespace TechChallenge.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObterPorIdAsync(Guid id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<T>> ObterTodosAsync()
        => await _dbSet.ToListAsync();

    public async Task AdicionarAsync(T entidade)
        => await _dbSet.AddAsync(entidade);

    public Task AtualizarAsync(T entidade)
    {
        _dbSet.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(T entidade)
    {
        _dbSet.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync()
        => await _context.SaveChangesAsync();
}
