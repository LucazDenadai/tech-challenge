using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OficinaMecanica.Estoque.IntegrationTests.Fixtures;

namespace OficinaMecanica.Estoque.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
public class PecasControllerTests : IAsyncLifetime
{
    private readonly EstoqueWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PecasControllerTests(EstoqueWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<PecaResponse> CriarPecaAsync(string nome = "Filtro de Óleo", int quantidade = 10)
    {
        var response = await _client.PostAsJsonAsync("/estoque/pecas", new
        {
            Nome = nome,
            Descricao = "Descrição da peça",
            Valor = 50.00m,
            QuantidadeEstoque = quantidade
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PecaResponse>())!;
    }

    // ── POST /estoque/pecas → 201 ─────────────────────────────────────────────

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornar201()
    {
        var response = await _client.PostAsJsonAsync("/estoque/pecas", new
        {
            Nome = "Pastilha de Freio",
            Descricao = "Pastilha dianteira",
            Valor = 120.00m,
            QuantidadeEstoque = 5
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criada = await response.Content.ReadFromJsonAsync<PecaResponse>();
        criada!.Nome.Should().Be("Pastilha de Freio");
        criada.Id.Should().NotBeEmpty();
    }

    // ── GET /estoque/pecas → 200 ──────────────────────────────────────────────

    [Fact]
    public async Task Listar_DeveRetornar200ComPecas()
    {
        await CriarPecaAsync("Vela de Ignição");

        var response = await _client.GetAsync("/estoque/pecas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<PecaResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    // ── GET /estoque/pecas/{id} → 200 ────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornar200()
    {
        var peca = await CriarPecaAsync("Amortecedor");

        var response = await _client.GetAsync($"/estoque/pecas/{peca.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<PecaResponse>();
        resultado!.Nome.Should().Be("Amortecedor");
    }

    // ── GET /estoque/pecas/{id} → 404 ────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync($"/estoque/pecas/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /estoque/pecas/{id} → 204 ────────────────────────────────────────

    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveRetornar204()
    {
        var peca = await CriarPecaAsync("Original");

        var response = await _client.PutAsJsonAsync($"/estoque/pecas/{peca.Id}", new
        {
            Id = peca.Id,
            Nome = "Atualizada",
            Descricao = "Nova descrição",
            Valor = 89.99m
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── DELETE /estoque/pecas/{id} → 204 ─────────────────────────────────────

    [Fact]
    public async Task Remover_DeveRetornar204ERemoverDaLista()
    {
        var peca = await CriarPecaAsync("Para Remover");

        var deleteResponse = await _client.DeleteAsync($"/estoque/pecas/{peca.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/estoque/pecas/{peca.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record PecaResponse(Guid Id, string Nome, string Descricao, decimal Valor, int QuantidadeEstoque);
}
