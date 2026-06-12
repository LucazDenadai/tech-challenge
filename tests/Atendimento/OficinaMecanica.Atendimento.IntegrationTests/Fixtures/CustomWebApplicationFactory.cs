using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Enums;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence;
using Testcontainers.PostgreSql;

namespace OficinaMecanica.Atendimento.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("oficina_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailPort));
            if (emailDescriptor is not null)
                services.Remove(emailDescriptor);
            services.AddScoped<IEmailPort, NoOpEmailAdapter>();

            var estoqueDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEstoquePort));
            if (estoqueDescriptor is not null)
                services.Remove(estoqueDescriptor);
            services.AddScoped<IEstoquePort, FakeEstoqueAdapter>();
        });

        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:Origins:0"] = "http://localhost"
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }
}

file class NoOpEmailAdapter : IEmailPort
{
    public Task EnviarAtualizacaoStatusAsync(string destinatario, string numeroOS, StatusOrdemServico novoStatus, CancellationToken ct = default)
        => Task.CompletedTask;
}

file class FakeEstoqueAdapter : IEstoquePort
{
    public Task<bool> VerificarDisponibilidadeAsync(IEnumerable<ItemPecaRequest> itens, CancellationToken ct = default)
        => Task.FromResult(true);

    public Task<PecaEstoqueDto?> ObterPecaAsync(Guid pecaId, CancellationToken ct = default)
        => Task.FromResult<PecaEstoqueDto?>(new(pecaId, "Peça Teste", "Descrição teste", 120m));
}
