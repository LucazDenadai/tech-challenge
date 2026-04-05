using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Data;

public class AppDbContext : DbContext
{
    [ExcludeFromCodeCoverage]
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Peca> Pecas => Set<Peca>();
    // Acessados internamente via context.Set<T>() pelos repositórios — não acessados diretamente
    [ExcludeFromCodeCoverage] public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
    [ExcludeFromCodeCoverage] public DbSet<ItemServico> ItensServico => Set<ItemServico>();
    [ExcludeFromCodeCoverage] public DbSet<ItemPeca> ItensPeca => Set<ItemPeca>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
