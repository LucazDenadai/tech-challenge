using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Application.DTOs.Cliente;
using TechChallenge.IntegrationTests.Fixtures;
using Xunit;

namespace TechChallenge.IntegrationTests.Controllers;

[Collection("Integration")]
public class ClientesControllerTests
{
    private readonly CustomWebApplicationFactory _factory;

    public ClientesControllerTests(CustomWebApplicationFactory factory)
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

        var response = await client.GetAsync("/api/clientes");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObterTodos_Autenticado_DeveRetornar200ComListaSeeded()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync("/api/clientes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await response.Content.ReadFromJsonAsync<IEnumerable<ClienteDto>>();
        lista.Should().NotBeNull();
        lista!.Should().HaveCountGreaterThanOrEqualTo(3); // 3 clientes do DbSeeder
    }

    [Fact]
    public async Task Criar_DadosValidos_DeveRetornar201()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarClienteDto
        {
            Nome = "Teste Integração",
            Cpf = "55566677788",
            Email = "integracao@teste.com",
            Telefone = "11900001111",
            Endereco = "Rua Teste, 999"
        };

        var response = await client.PostAsJsonAsync("/api/clientes", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<ClienteDto>();
        criado.Should().NotBeNull();
        criado!.Nome.Should().Be("Teste Integração");
        criado.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Criar_CpfDuplicado_DeveRetornar400()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarClienteDto
        {
            Nome = "Cliente Duplicado",
            Cpf = "12345678901", // CPF do Carlos Silva do DbSeeder
            Email = "duplicado@teste.com",
            Telefone = "11900002222",
            Endereco = "Rua Dup, 1"
        };

        var response = await client.PostAsJsonAsync("/api/clientes", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObterPorId_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.GetAsync($"/api/clientes/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Desativar_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.DeleteAsync($"/api/clientes/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Atualizar_ClienteExiste_DeveRetornar200()
    {
        var client = await CriarClienteAutenticadoAsync();

        // Criar cliente para atualizar
        var criar = new CriarClienteDto
        {
            Nome = "Para Atualizar",
            Cpf = "11122233300",
            Email = "atualizar@teste.com",
            Telefone = "11900001234",
            Endereco = "Rua Velha, 1"
        };
        var createResponse = await client.PostAsJsonAsync("/api/clientes", criar);
        var criado = await createResponse.Content.ReadFromJsonAsync<ClienteDto>();

        var dto = new CriarClienteDto
        {
            Nome = "Nome Atualizado",
            Cpf = "11122233300",
            Email = "atualizado@teste.com",
            Telefone = "11900009999",
            Endereco = "Rua Nova, 2"
        };

        var response = await client.PutAsJsonAsync($"/api/clientes/{criado!.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizado = await response.Content.ReadFromJsonAsync<ClienteDto>();
        atualizado!.Nome.Should().Be("Nome Atualizado");
    }

    [Fact]
    public async Task Atualizar_IdInexistente_DeveRetornar404()
    {
        var client = await CriarClienteAutenticadoAsync();
        var dto = new CriarClienteDto { Nome = "X", Cpf = "00000000001", Email = "x@x.com", Telefone = "11900000000", Endereco = "X" };

        var response = await client.PutAsJsonAsync($"/api/clientes/{Guid.NewGuid()}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FluxoCompleto_CriarObterDesativar_DevePassar()
    {
        var client = await CriarClienteAutenticadoAsync();

        // Criar
        var dto = new CriarClienteDto
        {
            Nome = "Fluxo Completo",
            Cpf = "99988877766",
            Email = "fluxo@teste.com",
            Telefone = "11911112222",
            Endereco = "Av. Fluxo, 100"
        };
        var createResponse = await client.PostAsJsonAsync("/api/clientes", dto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await createResponse.Content.ReadFromJsonAsync<ClienteDto>();

        // Obter por ID
        var getResponse = await client.GetAsync($"/api/clientes/{criado!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Desativar
        var deleteResponse = await client.DeleteAsync($"/api/clientes/{criado.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
