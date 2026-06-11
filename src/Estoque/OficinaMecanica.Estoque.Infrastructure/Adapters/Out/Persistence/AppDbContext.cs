using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.Out.Persistence;

public class AppDbContext : DbContext
{
    [ExcludeFromCodeCoverage]
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<MovimentacaoEstoque> Movimentacoes => Set<MovimentacaoEstoque>();
    public DbSet<FalhaProcessamento> Falhas => Set<FalhaProcessamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("estoque");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
