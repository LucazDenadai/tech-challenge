using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Atendimento.Domain.Entities;
using OficinaMecanica.Atendimento.Domain.Enums;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence;

namespace OficinaMecanica.Atendimento.IntegrationTests.Fixtures;

public static class AuthHelper
{
    private const string Email = "admin@test.com";
    private const string Senha = "Admin@123";

    // Mesmos valores de appsettings.json (ambiente de teste) — usados para simular o
    // JWT emitido pela Lambda de autenticação via CPF (CARD-29/30), sem depender de um
    // endpoint de login de cliente na própria app.
    private const string JwtKey = "placeholder-dev-key-minimum-32-chars-here!";
    private const string JwtIssuer = "oficina-atendimento";
    private const string JwtAudience = "oficina-atendimento-api";

    public static string GerarTokenCliente()
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Cliente"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

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
        var response = await client.PostAsJsonAsync("/atendimento/auth/login", new { Email, Senha });
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
