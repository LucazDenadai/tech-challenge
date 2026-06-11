using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OficinaMecanica.Estoque.IntegrationTests.Fixtures;

namespace OficinaMecanica.Estoque.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
public class DisponibilidadeControllerTests : IAsyncLifetime
{
    private readonly EstoqueWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DisponibilidadeControllerTests(EstoqueWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> CriarPecaAsync(string nome, int quantidade)
    {
        var response = await _client.PostAsJsonAsync("/estoque/pecas", new
        {
            Nome = nome,
            Descricao = "Descrição",
            Valor = 10.00m,
            QuantidadeEstoque = quantidade
        });
        response.EnsureSuccessStatusCode();
        var peca = await response.Content.ReadFromJsonAsync<PecaResponse>();
        return peca!.Id;
    }

    // ── POST /estoque/disponibilidade → true (todas disponíveis) ─────────────

    [Fact]
    public async Task ConsultarDisponibilidade_TodasDisponiveis_DeveRetornarTrue()
    {
        var id = await CriarPecaAsync("Filtro", 10);

        var response = await _client.PostAsJsonAsync("/estoque/disponibilidade", new
        {
            Itens = new[] { new { PecaId = id, QuantidadeSolicitada = 5 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<DisponibilidadeResponse>();
        resultado!.Disponivel.Should().BeTrue();
    }

    // ── POST /estoque/disponibilidade → false (estoque insuficiente) ──────────

    [Fact]
    public async Task ConsultarDisponibilidade_EstoqueInsuficiente_DeveRetornarFalse()
    {
        var id = await CriarPecaAsync("Rolamento", 2);

        var response = await _client.PostAsJsonAsync("/estoque/disponibilidade", new
        {
            Itens = new[] { new { PecaId = id, QuantidadeSolicitada = 10 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<DisponibilidadeResponse>();
        resultado!.Disponivel.Should().BeFalse();
    }

    private record PecaResponse(Guid Id);
    private record DisponibilidadeResponse(bool Disponivel);
}
