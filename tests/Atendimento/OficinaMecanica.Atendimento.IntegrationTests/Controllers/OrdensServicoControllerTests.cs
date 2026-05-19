using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Atendimento.Domain.Entities;
using OficinaMecanica.Atendimento.Domain.Enums;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence;
using OficinaMecanica.Atendimento.IntegrationTests.Fixtures;

namespace OficinaMecanica.Atendimento.IntegrationTests.Controllers;

[Collection(IntegrationTestCollection.Name)]
public class OrdensServicoControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrdensServicoControllerTests(CustomWebApplicationFactory factory)
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

    private int _seed = 0;

    private async Task<(Guid clienteId, Guid veiculoId)> SeedClienteVeiculoAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var n = Interlocked.Increment(ref _seed);
        var cpfs = new[] { "52998224725", "23708614526", "89540362369", "31792370075", "03863342690" };
        var placas = new[] { "ABC1D23", "DEF2E45", "GHI3F67", "JKL4G89", "MNO5H01" };

        var cliente = new Cliente($"Cliente {n}", cpfs[n % cpfs.Length], $"cliente{n}@test.com", "11999999999", "Rua A, 1");
        db.Clientes.Add(cliente);

        var veiculo = new Veiculo(cliente.Id, placas[n % placas.Length], "Toyota", "Corolla", 2020, "Prata");
        db.Veiculos.Add(veiculo);

        await db.SaveChangesAsync();
        return (cliente.Id, veiculo.Id);
    }

    private async Task<(Guid osId, Guid clienteId, Guid veiculoId)> AbrirOSAsync()
    {
        var (clienteId, veiculoId) = await SeedClienteVeiculoAsync();

        var body = new
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Observacoes = "Revisão completa",
            Pecas = Array.Empty<object>()
        };

        var response = await _client.PostAsJsonAsync("/ordens-servico", body);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var resultado = await response.Content.ReadFromJsonAsync<AbrirOSResponse>();
        return (resultado!.Id, clienteId, veiculoId);
    }

    private async Task MudarStatusAsync(Guid osId, StatusOrdemServico novoStatus)
    {
        var response = await _client.PutAsJsonAsync($"/ordens-servico/{osId}/status", new { NovoStatus = (int)novoStatus });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── Teste 1: Ciclo completo de uma OS ─────────────────────────────────────

    [Fact]
    public async Task CicloCompleto_DevePassarPortodosOsStatusEmOrdem()
    {
        var (osId, _, _) = await AbrirOSAsync();

        // Status inicial = Recebida
        var status = await _client.GetFromJsonAsync<ConsultarStatusOSResponse>($"/ordens-servico/{osId}/status");
        status!.Status.Should().Be(StatusOrdemServico.Recebida);

        // Recebida → EmDiagnostico
        await MudarStatusAsync(osId, StatusOrdemServico.EmDiagnostico);

        // EmDiagnostico → AguardandoAprovacao
        await MudarStatusAsync(osId, StatusOrdemServico.AguardandoAprovacao);

        // AprovarOrcamento → EmExecucao
        var aprovacao = await _client.PutAsJsonAsync($"/ordens-servico/{osId}/orcamento", new { Aprovado = true });
        aprovacao.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // EmExecucao → Finalizada
        await MudarStatusAsync(osId, StatusOrdemServico.Finalizada);

        // Confirma status final
        var final = await _client.GetFromJsonAsync<ConsultarStatusOSResponse>($"/ordens-servico/{osId}/status");
        final!.Status.Should().Be(StatusOrdemServico.Finalizada);
        final.Historico.Should().HaveCount(4);
    }

    // ── Teste 2: Listagem ordenada ────────────────────────────────────────────

    [Fact]
    public async Task Listar_DeveRetornarOrdensNaPrioridadeCorreta()
    {
        // OS1: Recebida
        var (os1Id, _, _) = await AbrirOSAsync();

        // OS2: EmDiagnostico
        var (os2Id, _, _) = await AbrirOSAsync();
        await MudarStatusAsync(os2Id, StatusOrdemServico.EmDiagnostico);

        // OS3: AguardandoAprovacao
        var (os3Id, _, _) = await AbrirOSAsync();
        await MudarStatusAsync(os3Id, StatusOrdemServico.EmDiagnostico);
        await MudarStatusAsync(os3Id, StatusOrdemServico.AguardandoAprovacao);

        // OS4: EmExecucao
        var (os4Id, _, _) = await AbrirOSAsync();
        await MudarStatusAsync(os4Id, StatusOrdemServico.EmDiagnostico);
        await MudarStatusAsync(os4Id, StatusOrdemServico.AguardandoAprovacao);
        var _ = await _client.PutAsJsonAsync($"/ordens-servico/{os4Id}/orcamento", new { Aprovado = true });

        // OS5: Finalizada (não deve aparecer)
        var (os5Id, _, _) = await AbrirOSAsync();
        await MudarStatusAsync(os5Id, StatusOrdemServico.EmDiagnostico);
        await MudarStatusAsync(os5Id, StatusOrdemServico.AguardandoAprovacao);
        await _client.PutAsJsonAsync($"/ordens-servico/{os5Id}/orcamento", new { Aprovado = true });
        await MudarStatusAsync(os5Id, StatusOrdemServico.Finalizada);

        var lista = await _client.GetFromJsonAsync<List<ListarOSItem>>("/ordens-servico");

        lista.Should().HaveCount(4);
        lista![0].Id.Should().Be(os4Id, "EmExecucao tem prioridade 1");
        lista[1].Id.Should().Be(os3Id, "AguardandoAprovacao tem prioridade 2");
        lista[2].Id.Should().Be(os2Id, "EmDiagnostico tem prioridade 3");
        lista[3].Id.Should().Be(os1Id, "Recebida tem prioridade 4");
    }

    // ── Teste 3: Aprovação de orçamento — caminho de erro ─────────────────────

    [Fact]
    public async Task AprovarOrcamento_QuandoStatusNaoEAguardandoAprovacao_DeveRetornar422()
    {
        var (osId, _, _) = await AbrirOSAsync();

        // OS está Recebida — aprovação deve falhar
        var response = await _client.PutAsJsonAsync($"/ordens-servico/{osId}/orcamento", new { Aprovado = true });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── Teste 4: Transição de status inválida ─────────────────────────────────

    [Fact]
    public async Task AtualizarStatus_ComTransicaoIlegal_DeveRetornar422()
    {
        var (osId, _, _) = await AbrirOSAsync();

        // Tentar ir direto de Recebida para Finalizada
        var response = await _client.PutAsJsonAsync($"/ordens-servico/{osId}/status", new { NovoStatus = (int)StatusOrdemServico.Finalizada });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── Records de apoio ──────────────────────────────────────────────────────

    private record AbrirOSResponse(Guid Id, string Numero);
    private record ConsultarStatusOSResponse(Guid Id, string Numero, StatusOrdemServico Status, decimal ValorTotal, List<HistoricoItem> Historico);
    private record HistoricoItem(StatusOrdemServico StatusAnterior, StatusOrdemServico StatusNovo, DateTime AlteradoEm);
    private record ListarOSItem(Guid Id, string Numero, StatusOrdemServico Status, DateTime DataAbertura, decimal ValorTotal);
}
