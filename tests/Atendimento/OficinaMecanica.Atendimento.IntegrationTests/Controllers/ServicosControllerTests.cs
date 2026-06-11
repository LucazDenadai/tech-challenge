using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OficinaMecanica.Atendimento.IntegrationTests.Fixtures;

namespace OficinaMecanica.Atendimento.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
public class ServicosControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ServicosControllerTests(CustomWebApplicationFactory factory)
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

    private async Task<ServicoResponse> CriarServicoAsync(string nome = "Troca de Óleo")
    {
        var response = await _client.PostAsJsonAsync("/servicos", new
        {
            Nome = nome,
            Descricao = "Troca completa do óleo do motor",
            Preco = 150.00m,
            TempoConclusaoMinutos = 60
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServicoResponse>())!;
    }

    // ── POST /servicos → 201 ─────────────────────────────────────────────────

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornar201()
    {
        var response = await _client.PostAsJsonAsync("/servicos", new
        {
            Nome = "Alinhamento",
            Descricao = "Alinhamento e balanceamento",
            Preco = 80.00m,
            TempoConclusaoMinutos = 45
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<ServicoResponse>();
        criado!.Nome.Should().Be("Alinhamento");
        criado.Ativo.Should().BeTrue();
    }

    // ── POST /servicos → 400 dados inválidos ─────────────────────────────────

    [Fact]
    public async Task Criar_ComDadosInvalidos_DeveRetornar400()
    {
        var response = await _client.PostAsJsonAsync("/servicos", new
        {
            Nome = "",
            Descricao = "Desc",
            Preco = -1m,
            TempoConclusaoMinutos = 0
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /servicos → 200 ──────────────────────────────────────────────────

    [Fact]
    public async Task Listar_DeveRetornar200ComServicos()
    {
        await CriarServicoAsync("Revisão Geral");

        var response = await _client.GetAsync("/servicos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<ServicoResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    // ── GET /servicos/{id} → 200 ─────────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornar200()
    {
        var servico = await CriarServicoAsync("Freios");

        var response = await _client.GetAsync($"/servicos/{servico.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ServicoResponse>();
        resultado!.Nome.Should().Be("Freios");
    }

    // ── GET /servicos/{id} → 404 ─────────────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync($"/servicos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /servicos/{id} → 200 ─────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveRetornar200()
    {
        var servico = await CriarServicoAsync("Original");

        var response = await _client.PutAsJsonAsync($"/servicos/{servico.Id}", new
        {
            Nome = "Atualizado",
            Descricao = "Nova descrição",
            Preco = 200.00m,
            TempoConclusaoMinutos = 90
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizado = await response.Content.ReadFromJsonAsync<ServicoResponse>();
        atualizado!.Nome.Should().Be("Atualizado");
        atualizado.Preco.Should().Be(200.00m);
    }

    // ── DELETE /servicos/{id} → 204 ──────────────────────────────────────────

    [Fact]
    public async Task Desativar_DeveRetornar204ERemoverDaLista()
    {
        var servico = await CriarServicoAsync("Para Desativar");

        var deleteResponse = await _client.DeleteAsync($"/servicos/{servico.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/servicos/{servico.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET sem token → 401 ──────────────────────────────────────────────────

    [Fact]
    public async Task Endpoint_SemToken_DeveRetornar401()
    {
        var clienteSemToken = _factory.CreateClient();

        var response = await clienteSemToken.GetAsync("/servicos");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record ServicoResponse(Guid Id, string Nome, string Descricao, decimal Preco, int TempoConclusaoMinutos, bool Ativo);
}
