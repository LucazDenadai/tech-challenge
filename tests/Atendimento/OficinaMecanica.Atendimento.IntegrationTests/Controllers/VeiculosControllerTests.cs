using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OficinaMecanica.Atendimento.IntegrationTests.Fixtures;

namespace OficinaMecanica.Atendimento.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
public class VeiculosControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public VeiculosControllerTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> CriarClienteAsync(string cpf, string email)
    {
        var response = await _client.PostAsJsonAsync("/atendimento/clientes", new
        {
            Nome = "Cliente Veiculo",
            Documento = cpf,
            Email = email,
            Telefone = "11999999999",
            Endereco = "Rua A, 1"
        });
        response.EnsureSuccessStatusCode();
        var criado = await response.Content.ReadFromJsonAsync<IdResponse>();
        return criado!.Id;
    }

    private async Task<VeiculoResponse> CriarVeiculoAsync(Guid clienteId, string placa)
    {
        var response = await _client.PostAsJsonAsync("/atendimento/veiculos", new
        {
            ClienteId = clienteId,
            Placa = placa,
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2020,
            Cor = "Prata"
        });
        response.EnsureSuccessStatusCode();
        var criado = await response.Content.ReadFromJsonAsync<IdResponse>();
        var get = await _client.GetFromJsonAsync<VeiculoResponse>($"/atendimento/veiculos/{criado!.Id}");
        return get!;
    }

    // ── POST /veiculos → 201 ─────────────────────────────────────────────────

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornar201()
    {
        var clienteId = await CriarClienteAsync("52998224725", "criar.v@test.com");

        var response = await _client.PostAsJsonAsync("/atendimento/veiculos", new
        {
            ClienteId = clienteId,
            Placa = "AAA1B11",
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2021,
            Cor = "Preto"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // ── POST /veiculos → 422 placa duplicada ──────────────────────────────────

    [Fact]
    public async Task Criar_ComPlacaDuplicada_DeveRetornar422()
    {
        var clienteId = await CriarClienteAsync("23708614526", "dupla.v@test.com");
        await CriarVeiculoAsync(clienteId, "BBB2C22");

        var response = await _client.PostAsJsonAsync("/atendimento/veiculos", new
        {
            ClienteId = clienteId,
            Placa = "BBB2C22",
            Marca = "Ford",
            Modelo = "Ka",
            Ano = 2019,
            Cor = "Branco"
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── GET /veiculos → 200 ──────────────────────────────────────────────────

    [Fact]
    public async Task Listar_DeveRetornar200ComVeiculos()
    {
        var clienteId = await CriarClienteAsync("89540362369", "lista.v@test.com");
        await CriarVeiculoAsync(clienteId, "CCC3D33");

        var response = await _client.GetAsync("/atendimento/veiculos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<VeiculoResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    // ── GET /veiculos/{id} → 200 e 404 ───────────────────────────────────────

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornar200()
    {
        var clienteId = await CriarClienteAsync("31792370075", "getid.v@test.com");
        var veiculo = await CriarVeiculoAsync(clienteId, "DDD4E44");

        var response = await _client.GetAsync($"/atendimento/veiculos/{veiculo.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<VeiculoResponse>();
        resultado!.Placa.Should().Be("DDD4E44");
    }

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync($"/atendimento/veiculos/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /veiculos/cliente/{clienteId} → 200 ──────────────────────────────

    [Fact]
    public async Task ObterPorCliente_DeveRetornarVeiculosDoCliente()
    {
        var clienteId = await CriarClienteAsync("03863342690", "porcliente.v@test.com");
        await CriarVeiculoAsync(clienteId, "EEE5F55");

        var response = await _client.GetAsync($"/atendimento/veiculos/cliente/{clienteId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<VeiculoResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
        lista!.All(v => v.ClienteId == clienteId).Should().BeTrue();
    }

    // ── GET /veiculos/cliente/documento/{doc} → 200 ───────────────────────────

    [Fact]
    public async Task ObterPorDocumentoCliente_DeveRetornarVeiculosDoCliente()
    {
        var clienteId = await CriarClienteAsync("52998224725", "pordoc.v@test.com");
        await CriarVeiculoAsync(clienteId, "FFF6G66");

        var response = await _client.GetAsync("/atendimento/veiculos/cliente/documento/52998224725");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<List<VeiculoResponse>>();
        lista.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    // ── PUT /veiculos/{id} → 200 ─────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveRetornar200()
    {
        var clienteId = await CriarClienteAsync("23708614526", "put.v@test.com");
        var veiculo = await CriarVeiculoAsync(clienteId, "GGG7H77");

        var response = await _client.PutAsJsonAsync($"/atendimento/veiculos/{veiculo.Id}", new
        {
            Marca = "Chevrolet",
            Modelo = "Onix",
            Ano = 2022,
            Cor = "Vermelho"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizado = await response.Content.ReadFromJsonAsync<VeiculoResponse>();
        atualizado!.Marca.Should().Be("Chevrolet");
        atualizado.Cor.Should().Be("Vermelho");
    }

    // ── DELETE /veiculos/{id} → 204 ──────────────────────────────────────────

    [Fact]
    public async Task Remover_SemOSVinculada_DeveRetornar204()
    {
        var clienteId = await CriarClienteAsync("89540362369", "del.v@test.com");
        var veiculo = await CriarVeiculoAsync(clienteId, "HHH8I88");

        var response = await _client.DeleteAsync($"/atendimento/veiculos/{veiculo.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private record IdResponse(Guid Id);
    private record VeiculoResponse(Guid Id, Guid ClienteId, string Placa, string Marca, string Modelo, int Ano, string Cor);
}
