using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TechChallenge.Application.DTOs.Auth;

namespace TechChallenge.IntegrationTests.Fixtures;

public static class AuthHelper
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Faz login com as credenciais do admin gerado pelo DbSeeder e retorna o token JWT.</summary>
    public static async Task<string> ObterTokenAdminAsync(HttpClient client)
        => await ObterTokenAsync(client, "admin@oficina.com", "Admin@1234");

    /// <summary>Faz login com as credenciais do mecânico gerado pelo DbSeeder.</summary>
    public static async Task<string> ObterTokenMecanicoAsync(HttpClient client)
        => await ObterTokenAsync(client, "mecanico@oficina.com", "Mecan@123");

    private static async Task<string> ObterTokenAsync(HttpClient client, string email, string senha)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Senha = senha });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<TokenResponseDto>(body, JsonOptions)
            ?? throw new InvalidOperationException("Falha ao desserializar token.");

        return token.Token;
    }

    public static void AdicionarToken(this HttpClient client, string token)
        => client.DefaultRequestHeaders.Authorization = new("Bearer", token);
}
