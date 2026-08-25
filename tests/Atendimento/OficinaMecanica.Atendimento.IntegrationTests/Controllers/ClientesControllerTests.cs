using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Atendimento.IntegrationTests.Fixtures;

namespace OficinaMecanica.Atendimento.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
public class ClientesControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ClientesControllerTests(CustomWebApplicationFactory factory)
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

    // ── Teste 5a: POST com CPF válido → 201 ───────────────────────────────────

    [Fact]
    public async Task CriarCliente_ComCpfValido_DeveRetornar201()
    {
        var body = new
        {
            Nome = "Maria Souza",
            Documento = "529.982.247-25",
            Email = "maria@test.com",
            Telefone = "11988887777",
            Endereco = "Rua B, 10"
        };

        var response = await _client.PostAsJsonAsync("/atendimento/clientes", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var resultado = await response.Content.ReadFromJsonAsync<CriarClienteResponse>();
        resultado!.Id.Should().NotBeEmpty();
    }

    // ── Teste 5b: POST com mesmo CPF → 422 ────────────────────────────────────

    [Fact]
    public async Task CriarCliente_ComCpfDuplicado_DeveRetornar422()
    {
        var body = new
        {
            Nome = "Pedro Lima",
            Documento = "52998224725",
            Email = "pedro@test.com",
            Telefone = "11977776666",
            Endereco = "Rua C, 20"
        };

        var primeira = await _client.PostAsJsonAsync("/atendimento/clientes", body);
        primeira.StatusCode.Should().Be(HttpStatusCode.Created);

        var segunda = await _client.PostAsJsonAsync("/atendimento/clientes", body with { Email = "pedro2@test.com" });
        segunda.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── Teste 5c: POST com CPF inválido → 400 ────────────────────────────────

    [Fact]
    public async Task CriarCliente_ComCpfInvalido_DeveRetornar400()
    {
        var body = new
        {
            Nome = "Fulano",
            Documento = "111.111.111-11",
            Email = "fulano@test.com",
            Telefone = "11966665555",
            Endereco = "Rua D, 30"
        };

        var response = await _client.PostAsJsonAsync("/atendimento/clientes", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Teste 5d: DELETE soft delete → Ativo = false ─────────────────────────

    [Fact]
    public async Task DesativarCliente_DeveFazerSoftDelete()
    {
        // Cria o cliente
        var body = new
        {
            Nome = "Carlos Mendes",
            Documento = "73959003404",
            Email = "carlos@test.com",
            Telefone = "11955554444",
            Endereco = "Rua E, 40"
        };

        var criarResponse = await _client.PostAsJsonAsync("/atendimento/clientes", body);
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await criarResponse.Content.ReadFromJsonAsync<CriarClienteResponse>();

        // Desativa
        var deleteResponse = await _client.DeleteAsync($"/atendimento/clientes/{criado!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificação direta no banco: Ativo = false
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.AppDbContext>();
        var clienteNoBanco = await db.Clientes.FindAsync(criado.Id);
        clienteNoBanco!.Ativo.Should().BeFalse();
    }

    // ── GET /clientes → 200 com lista ────────────────────────────────────────

    [Fact]
    public async Task Listar_DeveRetornar200ComListaDeClientes()
    {
        await _client.PostAsJsonAsync("/atendimento/clientes", new
        {
            Nome = "Lista Test",
            Documento = "52998224725",
            Email = "lista@test.com",
            Telefone = "11999999999",
            Endereco = "Rua A, 1"
        });

        var response = await _client.GetAsync("/atendimento/clientes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<ClienteResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    // ── GET /clientes?busca= → 200 filtrando ─────────────────────────────────

    [Fact]
    public async Task Listar_ComBusca_DeveRetornarApenasCorrespondentes()
    {
        await _client.PostAsJsonAsync("/atendimento/clientes", new
        {
            Nome = "Zilda Busca",
            Documento = "23708614526",
            Email = "zilda@test.com",
            Telefone = "11988888888",
            Endereco = "Rua B, 2"
        });

        var response = await _client.GetAsync("/atendimento/clientes?busca=Zilda");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<ClienteResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
        lista!.All(c => c.Nome.Contains("Zilda")).Should().BeTrue();
    }

    // ── GET /clientes/{id} → 200 ──────────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornar200()
    {
        var criar = await _client.PostAsJsonAsync("/atendimento/clientes", new
        {
            Nome = "Obter Por Id",
            Documento = "89540362369",
            Email = "obterid@test.com",
            Telefone = "11977777777",
            Endereco = "Rua C, 3"
        });
        var criado = await criar.Content.ReadFromJsonAsync<CriarClienteResponse>();

        var response = await _client.GetAsync($"/atendimento/clientes/{criado!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        cliente!.Nome.Should().Be("Obter Por Id");
    }

    // ── GET /clientes/{id} → 404 ──────────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync($"/atendimento/clientes/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /clientes/{id} → 200 com dados atualizados ───────────────────────

    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveRetornar200ComClienteAtualizado()
    {
        var criar = await _client.PostAsJsonAsync("/atendimento/clientes", new
        {
            Nome = "Original Nome",
            Documento = "31792370075",
            Email = "original@test.com",
            Telefone = "11966666666",
            Endereco = "Rua D, 4"
        });
        var criado = await criar.Content.ReadFromJsonAsync<CriarClienteResponse>();

        var response = await _client.PutAsJsonAsync($"/atendimento/clientes/{criado!.Id}", new
        {
            Nome = "Nome Atualizado",
            Email = "atualizado@test.com",
            Telefone = "11955555555",
            Endereco = "Rua E, 5"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizado = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        atualizado!.Nome.Should().Be("Nome Atualizado");
        atualizado.Email.Should().Be("atualizado@test.com");
    }

    private record CriarClienteResponse(Guid Id);
    private record ClienteResponse(Guid Id, string Nome, string Documento, string Email, string Telefone, string Endereco, bool Ativo);
}
