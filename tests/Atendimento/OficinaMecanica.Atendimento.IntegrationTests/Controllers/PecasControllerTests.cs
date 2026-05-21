using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OficinaMecanica.Atendimento.IntegrationTests.Fixtures;

namespace OficinaMecanica.Atendimento.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
public class PecasControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PecasControllerTests(CustomWebApplicationFactory factory)
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

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<PecaResponse> CriarPecaAsync(string nome = "Filtro de Óleo")
    {
        var response = await _client.PostAsJsonAsync("/pecas", new
        {
            Nome = nome,
            Descricao = "Filtro para troca de óleo",
            Preco = 45.00m
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PecaResponse>())!;
    }

    // ── POST /pecas → 201 ────────────────────────────────────────────────────

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornar201()
    {
        var response = await _client.PostAsJsonAsync("/pecas", new
        {
            Nome = "Pastilha de Freio",
            Descricao = "Pastilha dianteira",
            Preco = 120.00m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criada = await response.Content.ReadFromJsonAsync<PecaResponse>();
        criada!.Nome.Should().Be("Pastilha de Freio");
        criada.Ativo.Should().BeTrue();
    }

    // ── POST /pecas → 400 dados inválidos ────────────────────────────────────

    [Fact]
    public async Task Criar_ComDadosInvalidos_DeveRetornar400()
    {
        var response = await _client.PostAsJsonAsync("/pecas", new
        {
            Nome = "",
            Descricao = "Desc",
            Preco = -5m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /pecas → 200 ─────────────────────────────────────────────────────

    [Fact]
    public async Task Listar_DeveRetornar200ComPecas()
    {
        await CriarPecaAsync("Vela de Ignição");

        var response = await _client.GetAsync("/pecas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<PecaResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    // ── GET /pecas?nome= → 200 filtrando ─────────────────────────────────────

    [Fact]
    public async Task Listar_ComFiltroNome_DeveRetornarApenasCorrespondentes()
    {
        await CriarPecaAsync("Correia Dentada Especial");

        var response = await _client.GetAsync("/pecas?nome=Dentada");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<PecaResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
        lista!.All(p => p.Nome.Contains("Dentada")).Should().BeTrue();
    }

    // ── GET /pecas/{id} → 200 ────────────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornar200()
    {
        var peca = await CriarPecaAsync("Amortecedor");

        var response = await _client.GetAsync($"/pecas/{peca.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<PecaResponse>();
        resultado!.Nome.Should().Be("Amortecedor");
    }

    // ── GET /pecas/{id} → 404 ────────────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync($"/pecas/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /pecas/{id} → 200 ────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveRetornar200()
    {
        var peca = await CriarPecaAsync("Original");

        var response = await _client.PutAsJsonAsync($"/pecas/{peca.Id}", new
        {
            Nome = "Atualizada",
            Descricao = "Nova descrição",
            Preco = 89.99m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizada = await response.Content.ReadFromJsonAsync<PecaResponse>();
        atualizada!.Nome.Should().Be("Atualizada");
        atualizada.Preco.Should().Be(89.99m);
    }

    // ── DELETE /pecas/{id} → 204 ─────────────────────────────────────────────

    [Fact]
    public async Task Desativar_DeveRetornar204ERemoverDaLista()
    {
        var peca = await CriarPecaAsync("Para Desativar");

        var deleteResponse = await _client.DeleteAsync($"/pecas/{peca.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/pecas/{peca.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET sem token → 401 ──────────────────────────────────────────────────

    [Fact]
    public async Task Endpoint_SemToken_DeveRetornar401()
    {
        var clienteSemToken = _factory.CreateClient();

        var response = await clienteSemToken.GetAsync("/pecas");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record PecaResponse(Guid Id, string Nome, string Descricao, decimal Preco, bool Ativo);
}
