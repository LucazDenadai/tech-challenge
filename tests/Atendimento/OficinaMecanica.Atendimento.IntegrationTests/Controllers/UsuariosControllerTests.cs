using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Atendimento.Domain.Enums;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence;
using OficinaMecanica.Atendimento.IntegrationTests.Fixtures;

namespace OficinaMecanica.Atendimento.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
public class UsuariosControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsuariosControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await AuthHelper.SeedAdminAsync(_factory);
        var token = await AuthHelper.ObterTokenAsync(_client);
        AuthHelper.AplicarToken(_client, token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ── GET /usuarios → 200 com lista ────────────────────────────────────────

    [Fact]
    public async Task Listar_DeveRetornar200ComAdminSeedado()
    {
        var response = await _client.GetAsync("/atendimento/usuarios");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<UsuarioResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    // ── GET /usuarios?email= → 200 filtrando ────────────────────────────────

    [Fact]
    public async Task Listar_ComFiltroEmail_DeveRetornarApenasCorrespondentes()
    {
        await _client.PostAsJsonAsync("/atendimento/usuarios", new
        {
            Nome = "Joao Filtro",
            Email = "joao.filtro@test.com",
            Senha = "Senha@123",
            Perfil = (int)PerfilUsuario.Atendente
        });

        var response = await _client.GetAsync("/atendimento/usuarios?email=joao.filtro@test.com");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<UsuarioResponse>>();
        lista.Should().HaveCount(1);
        lista![0].Email.Should().Be("joao.filtro@test.com");
    }

    // ── GET /usuarios/{id} → 200 ─────────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornar200()
    {
        var criar = await _client.PostAsJsonAsync("/atendimento/usuarios", new
        {
            Nome = "Ana Obter",
            Email = "ana.obter@test.com",
            Senha = "Senha@123",
            Perfil = (int)PerfilUsuario.Mecanico
        });
        var criado = await criar.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response = await _client.GetAsync($"/atendimento/usuarios/{criado!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuario!.Email.Should().Be("ana.obter@test.com");
    }

    // ── GET /usuarios/{id} → 404 ─────────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync($"/atendimento/usuarios/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /usuarios → 201 ─────────────────────────────────────────────────

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornar201()
    {
        var response = await _client.PostAsJsonAsync("/atendimento/usuarios", new
        {
            Nome = "Carlos Novo",
            Email = "carlos.novo@test.com",
            Senha = "Senha@123",
            Perfil = (int)PerfilUsuario.Atendente
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        criado!.Email.Should().Be("carlos.novo@test.com");
        criado.Perfil.Should().Be(PerfilUsuario.Atendente);
    }

    // ── POST /usuarios → 422 com email duplicado ─────────────────────────────

    [Fact]
    public async Task Criar_ComEmailDuplicado_DeveRetornar422()
    {
        var body = new
        {
            Nome = "Duplicado",
            Email = "duplicado@test.com",
            Senha = "Senha@123",
            Perfil = (int)PerfilUsuario.Atendente
        };

        var primeira = await _client.PostAsJsonAsync("/atendimento/usuarios", body);
        primeira.StatusCode.Should().Be(HttpStatusCode.Created);

        var segunda = await _client.PostAsJsonAsync("/atendimento/usuarios", body with { Nome = "Duplicado 2" });
        segunda.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── POST /usuarios → 400 com senha fraca ─────────────────────────────────

    [Fact]
    public async Task Criar_ComSenhaFraca_DeveRetornar400()
    {
        var response = await _client.PostAsJsonAsync("/atendimento/usuarios", new
        {
            Nome = "Senha Fraca",
            Email = "senhafraca@test.com",
            Senha = "123456",
            Perfil = (int)PerfilUsuario.Atendente
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── POST /usuarios → 400 com email inválido ──────────────────────────────

    [Fact]
    public async Task Criar_ComEmailInvalido_DeveRetornar400()
    {
        var response = await _client.PostAsJsonAsync("/atendimento/usuarios", new
        {
            Nome = "Email Invalido",
            Email = "nao-e-um-email",
            Senha = "Senha@123",
            Perfil = (int)PerfilUsuario.Atendente
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PUT /usuarios/{id} → 200 ─────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveRetornar200()
    {
        var criar = await _client.PostAsJsonAsync("/atendimento/usuarios", new
        {
            Nome = "Original",
            Email = "original@test.com",
            Senha = "Senha@123",
            Perfil = (int)PerfilUsuario.Atendente
        });
        var criado = await criar.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response = await _client.PutAsJsonAsync($"/atendimento/usuarios/{criado!.Id}", new
        {
            Nome = "Atualizado",
            Email = "atualizado@test.com",
            Senha = (string?)null,
            Perfil = (int)PerfilUsuario.Mecanico
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizado = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        atualizado!.Nome.Should().Be("Atualizado");
        atualizado.Perfil.Should().Be(PerfilUsuario.Mecanico);
    }

    // ── DELETE /usuarios/{id} → 204 soft delete ──────────────────────────────

    [Fact]
    public async Task Desativar_DeveRetornar204EMarcarInativo()
    {
        var criar = await _client.PostAsJsonAsync("/atendimento/usuarios", new
        {
            Nome = "Para Desativar",
            Email = "desativar@test.com",
            Senha = "Senha@123",
            Perfil = (int)PerfilUsuario.Atendente
        });
        var criado = await criar.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response = await _client.DeleteAsync($"/atendimento/usuarios/{criado!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var noBanco = await db.Usuarios.FindAsync(criado.Id);
        noBanco!.Ativo.Should().BeFalse();
    }

    // ── Sem token → 401 ──────────────────────────────────────────────────────

    [Fact]
    public async Task Endpoint_SemToken_DeveRetornar401()
    {
        var clienteSemToken = _factory.CreateClient();

        var response = await clienteSemToken.GetAsync("/atendimento/usuarios");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record UsuarioResponse(Guid Id, string Nome, string Email, PerfilUsuario Perfil, bool Ativo);
}
