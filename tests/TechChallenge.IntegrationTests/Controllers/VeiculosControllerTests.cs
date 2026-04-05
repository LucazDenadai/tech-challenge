using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Application.DTOs.Cliente;
using TechChallenge.Application.DTOs.Veiculo;
using TechChallenge.IntegrationTests.Fixtures;
using Xunit;

namespace TechChallenge.IntegrationTests.Controllers;

[Collection("Integration")]
public class VeiculosControllerTests
{
    private readonly CustomWebApplicationFactory _factory;

    public VeiculosControllerTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> ObterPrimeiroClienteIdAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/clientes");
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<ClienteDto>>();
        return lista!.First().Id;
    }

    [Fact]
    public async Task ObterTodos_SemAutenticacao_DeveRetornar401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/veiculos");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObterTodos_Autenticado_DeveRetornar200ComVeiculosSeeded()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync("/api/veiculos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<VeiculoDto>>();
        lista.Should().NotBeNull();
        lista!.Should().HaveCountGreaterThanOrEqualTo(4); // 4 veículos do DbSeeder
    }

    [Fact]
    public async Task Criar_ClienteExiste_DeveRetornar201()
    {
        var client = await CriarClienteAutenticadoAsync();
        var clienteId = await ObterPrimeiroClienteIdAsync(client);
        var dto = new CriarVeiculoDto
        {
            ClienteId = clienteId,
            Placa = "TST0001",
            Marca = "Toyota",
            Modelo = "Yaris",
            Ano = 2023,
            Cor = "Azul"
        };

        var response = await client.PostAsJsonAsync("/api/veiculos", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<VeiculoDto>();
        criado.Should().NotBeNull();
        criado!.Placa.Should().Be("TST0001");
        criado.Marca.Should().Be("Toyota");
    }

    [Fact]
    public async Task Criar_ClienteInexistente_DeveRetornar400()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarVeiculoDto
        {
            ClienteId = Guid.NewGuid(),
            Placa = "TST9999",
            Marca = "Ford",
            Modelo = "Ka",
            Ano = 2020,
            Cor = "Prata"
        };

        var response = await client.PostAsJsonAsync("/api/veiculos", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObterPorId_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync($"/api/veiculos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObterPorCliente_DeveRetornarVeiculosDoCliente()
    {
        var client = await CriarClienteAutenticadoAsync();
        var clienteId = await ObterPrimeiroClienteIdAsync(client);

        var response = await client.GetAsync($"/api/veiculos/cliente/{clienteId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<VeiculoDto>>();
        lista.Should().NotBeNull();
        lista!.Should().AllSatisfy(v => v.ClienteId.Should().Be(clienteId));
    }

    [Fact]
    public async Task FluxoCompleto_CriarAtualizarRemover_DevePassar()
    {
        var client = await CriarClienteAutenticadoAsync();
        var clienteId = await ObterPrimeiroClienteIdAsync(client);

        // Criar
        var dto = new CriarVeiculoDto
        {
            ClienteId = clienteId,
            Placa = "FLX0001",
            Marca = "VW",
            Modelo = "Polo",
            Ano = 2022,
            Cor = "Branco"
        };
        var createResponse = await client.PostAsJsonAsync("/api/veiculos", dto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await createResponse.Content.ReadFromJsonAsync<VeiculoDto>();

        // Atualizar
        var dtoAtualizar = new CriarVeiculoDto
        {
            ClienteId = clienteId,
            Placa = "FLX0001",
            Marca = "VW",
            Modelo = "Polo",
            Ano = 2023,
            Cor = "Vermelho"
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/veiculos/{criado!.Id}", dtoAtualizar);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizado = await updateResponse.Content.ReadFromJsonAsync<VeiculoDto>();
        atualizado!.Cor.Should().Be("Vermelho");

        // Remover
        var deleteResponse = await client.DeleteAsync($"/api/veiculos/{criado.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Atualizar_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();
        var clienteId = await ObterPrimeiroClienteIdAsync(client);
        var dto = new CriarVeiculoDto { ClienteId = clienteId, Placa = "XXX0000", Marca = "X", Modelo = "X", Ano = 2020, Cor = "X" };

        var response = await client.PutAsJsonAsync($"/api/veiculos/{Guid.NewGuid()}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remover_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.DeleteAsync($"/api/veiculos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
