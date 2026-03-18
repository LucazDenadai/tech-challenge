using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Application.DTOs.Peca;
using TechChallenge.IntegrationTests.Fixtures;
using Xunit;

namespace TechChallenge.IntegrationTests.Controllers;

[Collection("Integration")]
public class PecasControllerTests
{
    private readonly CustomWebApplicationFactory _factory;

    public PecasControllerTests(CustomWebApplicationFactory factory)
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
    public async Task ObterTodos_Autenticado_DeveRetornarPecasSeeded()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync("/api/pecas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<PecaDto>>();
        lista.Should().NotBeNull();
        lista!.Should().HaveCountGreaterThanOrEqualTo(5); // 5 peças do DbSeeder
    }

    [Fact]
    public async Task Criar_DadosValidos_DeveRetornar201()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarPecaDto
        {
            Nome = "Peca de Teste",
            Descricao = "Descrição de teste",
            Preco = 99.99m,
            QuantidadeEstoque = 20
        };

        var response = await client.PostAsJsonAsync("/api/pecas", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<PecaDto>();
        criado.Should().NotBeNull();
        criado!.Nome.Should().Be("Peca de Teste");
        criado.QuantidadeEstoque.Should().Be(20);
    }

    [Fact]
    public async Task ObterPorId_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync($"/api/pecas/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Atualizar_PecaExiste_DeveRetornar200()
    {
        var client = await CriarClienteAutenticadoAsync();

        // Busca uma peça existente
        var listaResponse = await client.GetAsync("/api/pecas");
        var lista = await listaResponse.Content.ReadFromJsonAsync<IEnumerable<PecaDto>>();
        var peca = lista!.First();

        var dto = new CriarPecaDto
        {
            Nome = peca.Nome + " Atualizado",
            Descricao = peca.Descricao,
            Preco = peca.Preco + 10m,
            QuantidadeEstoque = 0 // sem adição de estoque
        };

        var response = await client.PutAsJsonAsync($"/api/pecas/{peca.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizado = await response.Content.ReadFromJsonAsync<PecaDto>();
        atualizado!.Nome.Should().Contain("Atualizado");
    }

    [Fact]
    public async Task Desativar_PecaExiste_DeveRetornar204()
    {
        var client = await CriarClienteAutenticadoAsync();

        // Cria uma peça para depois desativar
        var dto = new CriarPecaDto { Nome = "Para Desativar", Descricao = "Desc", Preco = 10m, QuantidadeEstoque = 5 };
        var createResponse = await client.PostAsJsonAsync("/api/pecas", dto);
        var criado = await createResponse.Content.ReadFromJsonAsync<PecaDto>();

        var response = await client.DeleteAsync($"/api/pecas/{criado!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
