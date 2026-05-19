using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence;

public class AppDbContext : DbContext
{
    [ExcludeFromCodeCoverage]
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Peca> Pecas => Set<Peca>();
    [ExcludeFromCodeCoverage] public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
    [ExcludeFromCodeCoverage] public DbSet<ItemServico> ItensServico => Set<ItemServico>();
    [ExcludeFromCodeCoverage] public DbSet<ItemPeca> ItensPeca => Set<ItemPeca>();
    [ExcludeFromCodeCoverage] public DbSet<HistoricoStatusOS> HistoricoStatusOS => Set<HistoricoStatusOS>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
