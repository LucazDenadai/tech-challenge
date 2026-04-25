using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Application.DTOs.Cliente;
using TechChallenge.Application.DTOs.OrdemServico;
using TechChallenge.Application.DTOs.Peca;
using TechChallenge.Application.DTOs.Servico;
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

    private static async Task<(string documento, string placa, Guid clienteId, Guid veiculoId)> ObterDocumentoEPlacaSeededAsync(HttpClient client)
    {
        var veiculosResponse = await client.GetAsync("/api/veiculos");
        var veiculos = await veiculosResponse.Content.ReadFromJsonAsync<IEnumerable<VeiculoDto>>();
        var veiculo = veiculos!.First();

        var clienteResponse = await client.GetAsync($"/api/clientes/{veiculo.ClienteId}");
        var cliente = await clienteResponse.Content.ReadFromJsonAsync<ClienteDto>(AuthHelper.JsonOptions);

        return (cliente!.Documento, veiculo.Placa, veiculo.ClienteId, veiculo.Id);
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
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<OrdemServicoDto>>(AuthHelper.JsonOptions);
        lista.Should().NotBeNull();
    }

    [Fact]
    public async Task Criar_DadosValidos_DeveRetornar201()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (documento, placa, clienteId, veiculoId) = await ObterDocumentoEPlacaSeededAsync(client);

        var dto = new CriarOrdemServicoDto
        {
            DocumentoCliente = documento,
            PlacaVeiculo = placa,
            Observacoes = "Revisão de rotina"
        };

        var response = await client.PostAsJsonAsync("/api/ordensservico", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);
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
    public async Task ObterPorBusca_DeveRetornarOrdensDoCliente()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (documento, placa, clienteId, _) = await ObterDocumentoEPlacaSeededAsync(client);

        // Criar uma OS para garantir que existe
        var dto = new CriarOrdemServicoDto { DocumentoCliente = documento, PlacaVeiculo = placa };
        await client.PostAsJsonAsync("/api/ordensservico", dto);

        var response = await client.GetAsync($"/api/ordensservico?busca={documento}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<OrdemServicoDto>>(AuthHelper.JsonOptions);
        lista.Should().NotBeNull();
        lista!.Should().AllSatisfy(o => o.ClienteId.Should().Be(clienteId));
    }

    [Fact]
    public async Task ObterPorStatus_DeveRetornar200()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync("/api/ordensservico?status=Recebida");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FluxoCompleto_CriarObterAvancarStatus_DevePassar()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (documento, placa, _, _) = await ObterDocumentoEPlacaSeededAsync(client);

        // Criar OS
        var criarDto = new CriarOrdemServicoDto { DocumentoCliente = documento, PlacaVeiculo = placa, Observacoes = "Fluxo completo" };
        var createResponse = await client.PostAsJsonAsync("/api/ordensservico", criarDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var os = await createResponse.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);
        os.Should().NotBeNull();

        // Obter por ID
        var getResponse = await client.GetAsync($"/api/ordensservico/{os!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Avançar status pelo número: Recebida → EmDiagnostico
        var statusDto = new AlterarStatusDto { NovoStatus = TechChallenge.Domain.Enums.StatusOrdemServico.EmDiagnostico };
        var avancarResponse = await client.PatchAsJsonAsync($"/api/ordensservico/{os.Numero}/status", statusDto, AuthHelper.JsonOptions);
        avancarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var osAvancada = await avancarResponse.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);
        osAvancada!.Status.Should().NotBe(os.Status);
    }

    [Fact]
    public async Task AvancarStatus_NumeroInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var statusDto = new AlterarStatusDto { NovoStatus = TechChallenge.Domain.Enums.StatusOrdemServico.EmDiagnostico };
        var response = await client.PatchAsJsonAsync("/api/ordensservico/OS-9999-9999/status", statusDto, AuthHelper.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdicionarServico_OSEmDiagnostico_DeveRetornar200()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (documento, placa, _, _) = await ObterDocumentoEPlacaSeededAsync(client);

        // Criar e avancar para EmDiagnostico
        var criarDto = new CriarOrdemServicoDto { DocumentoCliente = documento, PlacaVeiculo = placa };
        var createResponse = await client.PostAsJsonAsync("/api/ordensservico", criarDto);
        var os = await createResponse.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);

        var statusDto = new AlterarStatusDto { NovoStatus = TechChallenge.Domain.Enums.StatusOrdemServico.EmDiagnostico };
        var avancarResponse = await client.PatchAsJsonAsync($"/api/ordensservico/{os!.Numero}/status", statusDto, AuthHelper.JsonOptions);
        avancarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var osEmDiagnostico = await avancarResponse.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);
        osEmDiagnostico!.Status.Should().Be(TechChallenge.Domain.Enums.StatusOrdemServico.EmDiagnostico);

        // Obter um serviço seeded
        var servicosResponse = await client.GetAsync("/api/servicos");
        var servicos = await servicosResponse.Content.ReadFromJsonAsync<IEnumerable<ServicoDto>>();
        var servico = servicos!.First();

        // Adicionar serviço
        var adicionarDto = new AdicionarItemServicoDto { ServicoId = servico.Id };
        var response = await client.PostAsJsonAsync($"/api/ordensservico/{os.Id}/servicos", adicionarDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var osAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);
        osAtualizada!.ItensServico.Should().HaveCount(1);
        osAtualizada.ItensServico.First().ServicoId.Should().Be(servico.Id);
    }

    [Fact]
    public async Task AdicionarServico_OSEmRecebida_DeveRetornar400()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (documento, placa, _, _) = await ObterDocumentoEPlacaSeededAsync(client);

        // Criar OS (status = Recebida)
        var criarDto = new CriarOrdemServicoDto { DocumentoCliente = documento, PlacaVeiculo = placa };
        var createResponse = await client.PostAsJsonAsync("/api/ordensservico", criarDto);
        var os = await createResponse.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);

        // Obter um serviço
        var servicosResponse = await client.GetAsync("/api/servicos");
        var servicos = await servicosResponse.Content.ReadFromJsonAsync<IEnumerable<ServicoDto>>();
        var servico = servicos!.First();

        // Tentar adicionar serviço com OS em Recebida (deve falhar)
        var adicionarDto = new AdicionarItemServicoDto { ServicoId = servico.Id };
        var response = await client.PostAsJsonAsync($"/api/ordensservico/{os!.Id}/servicos", adicionarDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AdicionarPeca_OSEmDiagnostico_DeveConsumirEstoque()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (documento, placa, _, _) = await ObterDocumentoEPlacaSeededAsync(client);

        // Criar e avancar para EmDiagnostico
        var criarDto = new CriarOrdemServicoDto { DocumentoCliente = documento, PlacaVeiculo = placa };
        var createResponse = await client.PostAsJsonAsync("/api/ordensservico", criarDto);
        var os = await createResponse.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);

        var statusDto = new AlterarStatusDto { NovoStatus = TechChallenge.Domain.Enums.StatusOrdemServico.EmDiagnostico };
        var avancarResponse = await client.PatchAsJsonAsync($"/api/ordensservico/{os!.Numero}/status", statusDto, AuthHelper.JsonOptions);
        avancarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var osEmDiagnostico = await avancarResponse.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);
        osEmDiagnostico!.Status.Should().Be(TechChallenge.Domain.Enums.StatusOrdemServico.EmDiagnostico);

        // Obter uma peça seeded
        var pecasResponse = await client.GetAsync("/api/pecas");
        var pecas = await pecasResponse.Content.ReadFromJsonAsync<IEnumerable<PecaDto>>();
        var peca = pecas!.First();
        var estoqueAnterior = peca.QuantidadeEstoque;

        // Adicionar peça
        var adicionarDto = new AdicionarItemPecaDto { PecaId = peca.Id, Quantidade = 2 };
        var response = await client.PostAsJsonAsync($"/api/ordensservico/{os.Id}/pecas", adicionarDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var osAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);
        osAtualizada!.ItensPeca.Should().HaveCount(1);
        osAtualizada.ItensPeca.First().Quantidade.Should().Be(2);

        // Verificar que o estoque foi consumido
        var pecaAtualizada = await client.GetAsync($"/api/pecas/{peca.Id}");
        var pecaResponseDto = await pecaAtualizada.Content.ReadFromJsonAsync<PecaDto>();
        pecaResponseDto!.QuantidadeEstoque.Should().Be(estoqueAnterior - 2);
    }

    [Fact]
    public async Task AdicionarPeca_EstoqueInsuficiente_DeveRetornar400()
    {
        var client = await CriarClienteAutenticadoAsync();
        var (documento, placa, _, _) = await ObterDocumentoEPlacaSeededAsync(client);

        // Criar e avancar para EmDiagnostico
        var criarDto = new CriarOrdemServicoDto { DocumentoCliente = documento, PlacaVeiculo = placa };
        var createResponse = await client.PostAsJsonAsync("/api/ordensservico", criarDto);
        var os = await createResponse.Content.ReadFromJsonAsync<OrdemServicoDto>(AuthHelper.JsonOptions);

        var statusDto = new AlterarStatusDto { NovoStatus = TechChallenge.Domain.Enums.StatusOrdemServico.EmDiagnostico };
        var avancarResponse = await client.PatchAsJsonAsync($"/api/ordensservico/{os!.Numero}/status", statusDto, AuthHelper.JsonOptions);
        avancarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Obter uma peça seeded
        var pecasResponse = await client.GetAsync("/api/pecas");
        var pecas = await pecasResponse.Content.ReadFromJsonAsync<IEnumerable<PecaDto>>();
        var peca = pecas!.First();

        // Tentar adicionar mais peças que o estoque disponível
        var adicionarDto = new AdicionarItemPecaDto { PecaId = peca.Id, Quantidade = peca.QuantidadeEstoque + 100 };
        var response = await client.PostAsJsonAsync($"/api/ordensservico/{os.Id}/pecas", adicionarDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
