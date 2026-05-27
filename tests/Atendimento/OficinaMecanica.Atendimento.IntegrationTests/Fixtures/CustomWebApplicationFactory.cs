using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

            // Substitui EmailSmtpAdapter por stub no ambiente de testes
            var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailPort));
            if (emailDescriptor is not null)
                services.Remove(emailDescriptor);
            services.AddScoped<IEmailPort, NoOpEmailAdapter>();
        });

        builder.UseEnvironment("Test");
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
