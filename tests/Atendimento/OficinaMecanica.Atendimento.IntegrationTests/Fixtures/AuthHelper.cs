using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Atendimento.Domain.Entities;
using OficinaMecanica.Atendimento.Domain.Enums;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence;

namespace OficinaMecanica.Atendimento.IntegrationTests.Fixtures;

public static class AuthHelper
{
    private const string Email = "admin@test.com";
    private const string Senha = "Admin@123";

    public static async Task SeedAdminAsync(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Usuarios.Any(u => u.Email == Email))
            return;

        var hash = BCrypt.Net.BCrypt.HashPassword(Senha);
        var usuario = new Usuario("Admin", Email, hash, PerfilUsuario.Admin);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
    }

    public static async Task<string> ObterTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new { Email, Senha });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }

    public static void AplicarToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private record TokenResponse(string Token);
}
