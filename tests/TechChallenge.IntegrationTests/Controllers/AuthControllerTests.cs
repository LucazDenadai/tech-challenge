using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Application.DTOs.Auth;
using TechChallenge.IntegrationTests.Fixtures;
using Xunit;

namespace TechChallenge.IntegrationTests.Controllers;

[Collection("Integration")]
public class AuthControllerTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_CredenciaisValidas_DeveRetornar200ComToken()
    {
        var client = _factory.CreateClient();
        var dto = new LoginDto { Email = "admin@oficina.com", Senha = "Admin@123" };

        var response = await client.PostAsJsonAsync("/api/auth/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrEmpty();
        body.Email.Should().Be("admin@oficina.com");
        body.Perfil.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_SenhaErrada_DeveRetornar401()
    {
        var client = _factory.CreateClient();
        var dto = new LoginDto { Email = "admin@oficina.com", Senha = "SenhaErrada" };

        var response = await client.PostAsJsonAsync("/api/auth/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_EmailInexistente_DeveRetornar401()
    {
        var client = _factory.CreateClient();
        var dto = new LoginDto { Email = "naoexiste@email.com", Senha = "qualquer" };

        var response = await client.PostAsJsonAsync("/api/auth/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_PerfilMecanico_DeveRetornarPerfilCorreto()
    {
        var client = _factory.CreateClient();
        var dto = new LoginDto { Email = "mecanico@oficina.com", Senha = "Mec@123" };

        var response = await client.PostAsJsonAsync("/api/auth/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        body!.Perfil.Should().Be("Mecanico");
    }
}
