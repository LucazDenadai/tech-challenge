using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace TechChallenge.IntegrationTests.Fixtures;

/// <summary>
/// Factory que sobe a API com um banco PostgreSQL real via Testcontainers.
/// Substitui a connection string e as configurações JWT para testes.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Usa a mesma chave do appsettings.json para garantir que o middleware de validação
    // (que pode ler diretamente do appsettings antes da override) e o AuthService
    // (que lê via Environment.GetEnvironmentVariable) usem a mesma chave.
    private const string TestJwtKey = "TechChallenge_SuperSecretKey_2024_Oficina_Mecanica!";
    private const string TestJwtIssuer = "TechChallenge.API";
    private const string TestJwtAudience = "TechChallenge.Client";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("techchallenge_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // AuthService lê JWT via Environment.GetEnvironmentVariable
        Environment.SetEnvironmentVariable("JWT_KEY", TestJwtKey);
        Environment.SetEnvironmentVariable("JWT_ISSUER", TestJwtIssuer);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", TestJwtAudience);
        Environment.SetEnvironmentVariable("JWT_EXPIRACAO_MINUTOS", "60");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                // JWT middleware de validação usa builder.Configuration["Jwt:Key"]
                ["Jwt:Key"] = TestJwtKey,
                ["Jwt:Issuer"] = TestJwtIssuer,
                ["Jwt:Audience"] = TestJwtAudience,
            });
        });
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("JWT_KEY", null);
        Environment.SetEnvironmentVariable("JWT_ISSUER", null);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);
        Environment.SetEnvironmentVariable("JWT_EXPIRACAO_MINUTOS", null);

        await _dbContainer.DisposeAsync();
    }
}
