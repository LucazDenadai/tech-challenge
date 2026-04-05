using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Application.DTOs.Cliente;
using TechChallenge.Application.DTOs.OrdemServico;
using TechChallenge.Application.DTOs.Veiculo;
using TechChallenge.IntegrationTests.Fixtures;
using Xunit;

namespace TechChallenge.IntegrationTests.Controllers;

[Collection("Integration")]
public class OrdensServicoControllerTests
{
    private readonly CustomWebApplicationFactory _factory;

    public OrdensServicoControllerTests(CustomWebApplicationFactory factory)
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

    private async Task<(Guid clienteId, Guid veiculoId)> ObterClienteEVeiculoSeededAsync(HttpClient client)
    {
        var clientesResponse = await client.GetAsync("/api/clientes");
        var clientes = await clientesResponse.Content.ReadFromJsonAsync<IEnumerable<ClienteDto>>();
        var clienteId = clientes!.First().Id;

        var veiculosResponse = await client.GetAsync($"/api/veiculos/cliente/{clienteId}");
        var veiculos = await veiculosResponse.Content.ReadFromJsonAsync<IEnumerable<VeiculoDto>>();
        var veiculoId = veiculos!.First().Id;

        return (clienteId, veiculoId);
    }

    [Fact]
    public async Task ObterTodos_SemAutenticacao_DeveRetornar401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ordensservico");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObterTodos_Autenticado_DeveRetornar200()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync("/api/ordensservico");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<OrdemServicoDto>>();
        lista.Should().NotBeNull();
    }

    [Fact]
    public async Task Criar_DadosValidos_DeveRetornar201()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (clienteId, veiculoId) = await ObterClienteEVeiculoSeededAsync(client);

        var dto = new CriarOrdemServicoDto
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Observacoes = "Revisão de rotina"
        };

        var response = await client.PostAsJsonAsync("/api/ordensservico", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<OrdemServicoDto>();
        criado.Should().NotBeNull();
        criado!.ClienteId.Should().Be(clienteId);
        criado.VeiculoId.Should().Be(veiculoId);
        criado.Observacoes.Should().Be("Revisão de rotina");
        criado.Numero.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ObterPorId_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync($"/api/ordensservico/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObterPorCliente_DeveRetornarOrdensDoCliente()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (clienteId, veiculoId) = await ObterClienteEVeiculoSeededAsync(client);

        // Criar uma OS para garantir que existe
        var dto = new CriarOrdemServicoDto { ClienteId = clienteId, VeiculoId = veiculoId };
        await client.PostAsJsonAsync("/api/ordensservico", dto);

        var response = await client.GetAsync($"/api/ordensservico/cliente/{clienteId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<OrdemServicoDto>>();
        lista.Should().NotBeNull();
        lista!.Should().AllSatisfy(o => o.ClienteId.Should().Be(clienteId));
    }

    [Fact]
    public async Task ObterPorStatus_DeveRetornar200()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync("/api/ordensservico/status/Recebida");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FluxoCompleto_CriarObterAvancarStatus_DevePassar()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (clienteId, veiculoId) = await ObterClienteEVeiculoSeededAsync(client);

        // Criar OS
        var criarDto = new CriarOrdemServicoDto { ClienteId = clienteId, VeiculoId = veiculoId, Observacoes = "Fluxo completo" };
        var createResponse = await client.PostAsJsonAsync("/api/ordensservico", criarDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var os = await createResponse.Content.ReadFromJsonAsync<OrdemServicoDto>();
        os.Should().NotBeNull();

        // Obter por ID
        var getResponse = await client.GetAsync($"/api/ordensservico/{os!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Avançar status: Recebida → EmDiagnostico
        var avancarResponse = await client.PatchAsync($"/api/ordensservico/{os.Id}/avancar-status", null);
        avancarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var osAvancada = await avancarResponse.Content.ReadFromJsonAsync<OrdemServicoDto>();
        osAvancada!.Status.Should().NotBe(os.Status);
    }

    [Fact]
    public async Task AvancarStatus_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.PatchAsync($"/api/ordensservico/{Guid.NewGuid()}/avancar-status", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
