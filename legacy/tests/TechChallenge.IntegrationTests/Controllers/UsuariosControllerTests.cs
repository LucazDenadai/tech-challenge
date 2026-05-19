using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Application.DTOs.Usuario;
using TechChallenge.Domain.Enums;
using TechChallenge.IntegrationTests.Fixtures;
using Xunit;

namespace TechChallenge.IntegrationTests.Controllers;

[Collection("Integration")]
public class UsuariosControllerTests
{
    private readonly CustomWebApplicationFactory _factory;

    public UsuariosControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CriarClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.ObterTokenAdminAsync(client);
        client.AdicionarToken(token);
        return client;
    }

    [Fact]
    public async Task ObterTodos_SemAutenticacao_DeveRetornar401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/usuarios");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObterTodos_Autenticado_DeveRetornar200ComUsuariosSeeded()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync("/api/usuarios");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<UsuarioDto>>(AuthHelper.JsonOptions);
        lista.Should().NotBeNull();
        lista!.Should().HaveCountGreaterThanOrEqualTo(3); // 3 usuários do DbSeeder
    }

    [Fact]
    public async Task Criar_DadosValidos_DeveRetornar201()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarUsuarioDto
        {
            Nome = "Usuario Teste",
            Email = $"usuario.teste.{Guid.NewGuid():N}@email.com",
            Senha = "Senha@123",
            Perfil = PerfilUsuario.Mecanico
        };

        var response = await client.PostAsJsonAsync("/api/usuarios", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<UsuarioDto>(AuthHelper.JsonOptions);
        criado.Should().NotBeNull();
        criado!.Nome.Should().Be("Usuario Teste");
        criado.Perfil.Should().Be(PerfilUsuario.Mecanico);
        criado.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Criar_EmailDuplicado_DeveRetornar400()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarUsuarioDto
        {
            Nome = "Admin Duplicado",
            Email = "admin@oficina.com", // email do DbSeeder
            Senha = "Senha@123",
            Perfil = PerfilUsuario.Admin
        };

        var response = await client.PostAsJsonAsync("/api/usuarios", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObterPorId_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync($"/api/usuarios/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FluxoCompleto_CriarObterDesativar_DevePassar()
    {
        var client = await CriarClienteAutenticadoAsync();

        // Criar
        var dto = new CriarUsuarioDto
        {
            Nome = "Fluxo Completo",
            Email = $"fluxo.{Guid.NewGuid():N}@email.com",
            Senha = "Fluxo@123",
            Perfil = PerfilUsuario.Atendente
        };
        var createResponse = await client.PostAsJsonAsync("/api/usuarios", dto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await createResponse.Content.ReadFromJsonAsync<UsuarioDto>(AuthHelper.JsonOptions);

        // Obter por ID
        var getResponse = await client.GetAsync($"/api/usuarios/{criado!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var obtido = await getResponse.Content.ReadFromJsonAsync<UsuarioDto>(AuthHelper.JsonOptions);
        obtido!.Nome.Should().Be("Fluxo Completo");

        // Desativar
        var deleteResponse = await client.DeleteAsync($"/api/usuarios/{criado.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Atualizar_UsuarioExiste_DeveRetornar200()
    {
        var client = await CriarClienteAutenticadoAsync();

        // Criar usuário para atualizar
        var email = $"atualizar.{Guid.NewGuid():N}@email.com";
        var criar = new CriarUsuarioDto { Nome = "Para Atualizar", Email = email, Senha = "Senha@123", Perfil = PerfilUsuario.Mecanico };
        var createResponse = await client.PostAsJsonAsync("/api/usuarios", criar);
        var criado = await createResponse.Content.ReadFromJsonAsync<UsuarioDto>(AuthHelper.JsonOptions) ?? new UsuarioDto();

        var dto = new CriarUsuarioDto { Nome = "Nome Atualizado", Email = email, Senha = "NovaSenha@123", Perfil = PerfilUsuario.Admin };

        var response = await client.PutAsJsonAsync($"/api/usuarios/{criado.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizado = await response.Content.ReadFromJsonAsync<UsuarioDto>(AuthHelper.JsonOptions);
        atualizado!.Nome.Should().Be("Nome Atualizado");
        atualizado.Perfil.Should().Be(PerfilUsuario.Admin);
    }

    [Fact]
    public async Task Atualizar_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarUsuarioDto { Nome = "X", Email = "x@x.com", Senha = "Senha@123", Perfil = PerfilUsuario.Mecanico };

        var response = await client.PutAsJsonAsync($"/api/usuarios/{Guid.NewGuid()}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Desativar_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.DeleteAsync($"/api/usuarios/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
