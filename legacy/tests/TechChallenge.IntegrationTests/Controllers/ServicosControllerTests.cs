using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Application.DTOs.Servico;
using TechChallenge.IntegrationTests.Fixtures;
using Xunit;

namespace TechChallenge.IntegrationTests.Controllers;

[Collection("Integration")]
public class ServicosControllerTests
{
    private readonly CustomWebApplicationFactory _factory;

    public ServicosControllerTests(CustomWebApplicationFactory factory)
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

        var response = await client.GetAsync("/api/servicos");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObterTodos_Autenticado_DeveRetornar200ComServicosSeeded()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync("/api/servicos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<ServicoDto>>();
        lista.Should().NotBeNull();
        lista!.Should().HaveCountGreaterThanOrEqualTo(5); // 5 serviços do DbSeeder
    }

    [Fact]
    public async Task Criar_DadosValidos_DeveRetornar201()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarServicoDto
        {
            Nome = "Serviço de Teste",
            Descricao = "Descrição de teste",
            Preco = 150m,
            TempoConclusaoMinutos = 60
        };

        var response = await client.PostAsJsonAsync("/api/servicos", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<ServicoDto>();
        criado.Should().NotBeNull();
        criado!.Nome.Should().Be("Serviço de Teste");
        criado.Preco.Should().Be(150m);
        criado.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task ObterPorId_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync($"/api/servicos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Atualizar_ServicoExiste_DeveRetornar200()
    {
        var client = await CriarClienteAutenticadoAsync();

        var listaResponse = await client.GetAsync("/api/servicos");
        var lista = await listaResponse.Content.ReadFromJsonAsync<IEnumerable<ServicoDto>>();
        var servico = lista!.First();

        var dto = new CriarServicoDto
        {
            Nome = servico.Nome + " Atualizado",
            Descricao = servico.Descricao,
            Preco = servico.Preco + 10m,
            TempoConclusaoMinutos = servico.TempoConclusaoMinutos
        };

        var response = await client.PutAsJsonAsync($"/api/servicos/{servico.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizado = await response.Content.ReadFromJsonAsync<ServicoDto>();
        atualizado!.Nome.Should().Contain("Atualizado");
    }

    [Fact]
    public async Task Desativar_ServicoExiste_DeveRetornar204()
    {
        var client = await CriarClienteAutenticadoAsync();

        var dto = new CriarServicoDto { Nome = "Para Desativar", Descricao = "Desc", Preco = 50m, TempoConclusaoMinutos = 15 };
        var createResponse = await client.PostAsJsonAsync("/api/servicos", dto);
        var criado = await createResponse.Content.ReadFromJsonAsync<ServicoDto>();

        var response = await client.DeleteAsync($"/api/servicos/{criado!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Desativar_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.DeleteAsync($"/api/servicos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Atualizar_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarServicoDto { Nome = "X", Descricao = "X", Preco = 1m, TempoConclusaoMinutos = 1 };

        var response = await client.PutAsJsonAsync($"/api/servicos/{Guid.NewGuid()}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
