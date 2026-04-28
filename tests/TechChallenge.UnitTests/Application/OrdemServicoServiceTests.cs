using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TechChallenge.Application.DTOs.OrdemServico;
using TechChallenge.Application.Services;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Interfaces;
using Xunit;

namespace TechChallenge.UnitTests.Application;

public class OrdemServicoServiceTests
{
    private readonly Mock<IOrdemServicoRepository> _repoMock = new();
    private readonly Mock<IServicoRepository> _servicoRepoMock = new();
    private readonly Mock<IPecaRepository> _pecaRepoMock = new();
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<IVeiculoRepository> _veiculoRepoMock = new();
    private readonly OrdemServicoService _sut;

    public OrdemServicoServiceTests()
    {
        var logger = new Mock<ILogger<OrdemServicoService>>();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        _sut = new OrdemServicoService(
            _repoMock.Object,
            _servicoRepoMock.Object,
            _pecaRepoMock.Object,
            _clienteRepoMock.Object,
            _veiculoRepoMock.Object,
            logger.Object,
            httpContextAccessor.Object);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaMapeada()
    {
        var lista = new[]
        {
            new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), ""),
            new OrdemServico("OS-002", Guid.NewGuid(), Guid.NewGuid(), ""),
        };
        _repoMock.Setup(r => r.ObterTodosAsync()).ReturnsAsync(lista);

        var resultado = await _sut.ObterTodosAsync();

        resultado.Should().HaveCount(2);
        resultado.First().Numero.Should().Be("OS-001");
    }

    [Fact]
    public async Task ObterPorIdAsync_OrdemExiste_DeveRetornarDto()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);

        var resultado = await _sut.ObterPorIdAsync(os.Id);

        resultado.Should().NotBeNull();
        resultado!.Numero.Should().Be("OS-001");
    }

    [Fact]
    public async Task ObterPorIdAsync_OrdemNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.ObterComDetalhesAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        var resultado = await _sut.ObterPorIdAsync(Guid.NewGuid());

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterPorClienteAsync_DeveRetornarOrdensDoCliente()
    {
        var clienteId = Guid.NewGuid();
        var lista = new[]
        {
            new OrdemServico("OS-001", clienteId, Guid.NewGuid(), ""),
            new OrdemServico("OS-002", clienteId, Guid.NewGuid(), ""),
        };
        _repoMock.Setup(r => r.ObterPorClienteAsync(clienteId)).ReturnsAsync(lista);

        var resultado = await _sut.ObterPorClienteAsync(clienteId);

        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(o => o.ClienteId.Should().Be(clienteId));
    }

    [Fact]
    public async Task ObterPorStatusAsync_DeveRetornarOrdensFiltradas()
    {
        var lista = new[]
        {
            new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), ""),
        };
        _repoMock.Setup(r => r.ObterPorStatusAsync(StatusOrdemServico.Recebida)).ReturnsAsync(lista);

        var resultado = await _sut.ObterPorStatusAsync(StatusOrdemServico.Recebida);

        resultado.Should().HaveCount(1);
        resultado.First().Status.Should().Be(StatusOrdemServico.Recebida);
    }

    [Fact]
    public async Task CriarAsync_DadosValidos_DeveCriarERetornarDto()
    {
        var cliente = new Cliente("Carlos", "52998224725", "c@c.com", "11999999999", "Rua A");
        var veiculo = new Veiculo(cliente.Id, "ABC1234", "Toyota", "Corolla", 2020, "Prata");
        var dto = new CriarOrdemServicoDto
        {
            DocumentoCliente = "52998224725",
            PlacaVeiculo = "ABC1234",
            Observacoes = "Revisão geral"
        };

        _clienteRepoMock.Setup(r => r.ObterPorDocumentoAsync("52998224725")).ReturnsAsync(cliente);
        _veiculoRepoMock.Setup(r => r.ObterPorPlacaAsync("ABC1234")).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.GerarNumeroAsync()).ReturnsAsync("OS-2026-0001");
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.CriarAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Numero.Should().Be("OS-2026-0001");
    }

    [Fact]
    public async Task CriarAsync_ClienteNaoEncontrado_DeveLancarExcecao()
    {
        var dto = new CriarOrdemServicoDto { DocumentoCliente = "52998224725", PlacaVeiculo = "ABC1234" };
        _clienteRepoMock.Setup(r => r.ObterPorDocumentoAsync("52998224725")).ReturnsAsync((Cliente?)null);

        var act = async () => await _sut.CriarAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Dados inválidos*");
    }

    [Fact]
    public async Task CriarAsync_VeiculoNaoPertenceAoCliente_DeveLancarExcecao()
    {
        var outroClienteId = Guid.NewGuid();
        var cliente = new Cliente("Carlos", "52998224725", "c@c.com", "11999999999", "Rua A");
        var veiculo = new Veiculo(outroClienteId, "ABC1234", "Toyota", "Corolla", 2020, "Prata");
        var dto = new CriarOrdemServicoDto { DocumentoCliente = "52998224725", PlacaVeiculo = "ABC1234" };

        _clienteRepoMock.Setup(r => r.ObterPorDocumentoAsync("52998224725")).ReturnsAsync(cliente);
        _veiculoRepoMock.Setup(r => r.ObterPorPlacaAsync("ABC1234")).ReturnsAsync(veiculo);

        var act = async () => await _sut.CriarAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*veículo*");
    }

    [Fact]
    public async Task AlterarStatusAsync_OrdemNaoEncontrada_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterComDetalhesPorNumeroAsync(It.IsAny<string>())).ReturnsAsync((OrdemServico?)null);

        var act = async () => await _sut.AlterarStatusAsync("OS-2026-0001", new AlterarStatusDto { NovoStatus = StatusOrdemServico.EmDiagnostico });

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Ordem de Serviço não encontrada*");
    }

    [Fact]
    public async Task AlterarStatusAsync_OrdemExiste_DeveAlterarStatus()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        _repoMock.Setup(r => r.ObterComDetalhesPorNumeroAsync("OS-001")).ReturnsAsync(os);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AlterarStatusAsync("OS-001", new AlterarStatusDto { NovoStatus = StatusOrdemServico.EmDiagnostico });

        resultado.Status.Should().Be(StatusOrdemServico.EmDiagnostico);
    }

    [Fact]
    public async Task AlterarStatusAsync_StatusInvalido_DeveLancarExcecao()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        _repoMock.Setup(r => r.ObterComDetalhesPorNumeroAsync("OS-001")).ReturnsAsync(os);

        var act = async () => await _sut.AlterarStatusAsync("OS-001", new AlterarStatusDto { NovoStatus = StatusOrdemServico.Entregue });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AdicionarItemAsync_OrdemNaoEncontrada_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterComDetalhesAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        var act = async () => await _sut.AdicionarItemAsync(Guid.NewGuid(), new AdicionarItemDto { Tipo = TipoItem.Servico });

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Ordem de Serviço não encontrada*");
    }

    [Fact]
    public async Task AdicionarItemAsync_TipoServico_ServicoNaoEncontrado_DeveLancarExcecao()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var dto = new AdicionarItemDto { Tipo = TipoItem.Servico, ItemId = Guid.NewGuid(), Quantidade = 1 };
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _servicoRepoMock.Setup(r => r.ObterPorIdAsync(dto.ItemId)).ReturnsAsync((Servico?)null);

        var act = async () => await _sut.AdicionarItemAsync(os.Id, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Serviço não encontrado*");
    }

    [Fact]
    public async Task AdicionarItemAsync_TipoServico_DadosValidos_DeveAdicionarItem()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var servico = new Servico("Troca de Óleo", "Desc", 80m, 30);
        var dto = new AdicionarItemDto { Tipo = TipoItem.Servico, ItemId = servico.Id, Quantidade = 2 };
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _servicoRepoMock.Setup(r => r.ObterPorIdAsync(servico.Id)).ReturnsAsync(servico);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AdicionarItemAsync(os.Id, dto);

        resultado.ItensServico.Should().HaveCount(1);
        resultado.ValorTotal.Should().Be(160m);
    }

    [Fact]
    public async Task AdicionarItemAsync_TipoPeca_EstoqueInsuficiente_DeveLancarExcecao()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var peca = new Peca("Filtro", "Desc", 35m, 1);
        var dto = new AdicionarItemDto { Tipo = TipoItem.Peca, ItemId = peca.Id, Quantidade = 5 };
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id)).ReturnsAsync(peca);

        var act = async () => await _sut.AdicionarItemAsync(os.Id, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Estoque insuficiente*");
    }

    [Fact]
    public async Task AdicionarItemAsync_TipoPeca_DadosValidos_DeveAdicionarEConsumirEstoque()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var peca = new Peca("Filtro", "Desc", 35m, 10);
        var dto = new AdicionarItemDto { Tipo = TipoItem.Peca, ItemId = peca.Id, Quantidade = 3 };
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id)).ReturnsAsync(peca);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _pecaRepoMock.Setup(r => r.AtualizarAsync(It.IsAny<Peca>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AdicionarItemAsync(os.Id, dto);

        resultado.ItensPeca.Should().HaveCount(1);
        peca.QuantidadeEstoque.Should().Be(7);
    }

    [Fact]
    public async Task ObterTempoMedioExecucaoAsync_SemOrdensFinalizadas_DeveRetornarZero()
    {
        _repoMock.Setup(r => r.ObterTempoMedioExecucaoAsync()).ReturnsAsync((0.0, 0));

        var resultado = await _sut.ObterTempoMedioExecucaoAsync();

        resultado.TempoMedioHoras.Should().Be(0);
        resultado.TotalOrdensAnalisadas.Should().Be(0);
    }

    [Fact]
    public async Task ObterTempoMedioExecucaoAsync_ComOrdensFinalizadas_DeveRetornarMedia()
    {
        _repoMock.Setup(r => r.ObterTempoMedioExecucaoAsync()).ReturnsAsync((5.5, 3));

        var resultado = await _sut.ObterTempoMedioExecucaoAsync();

        resultado.TempoMedioHoras.Should().Be(5.5);
        resultado.TotalOrdensAnalisadas.Should().Be(3);
    }

    // ── FiltrarAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task FiltrarAsync_DeveRetornarResultadosDoRepo()
    {
        var lista = new[] { new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "") };
        _repoMock.Setup(r => r.FiltrarAsync("OS", null)).ReturnsAsync(lista);

        var resultado = await _sut.FiltrarAsync("OS", null);

        resultado.Should().HaveCount(1);
        resultado.First().Numero.Should().Be("OS-001");
    }

    // ── CancelarItemAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task CancelarItemAsync_OrdemNaoEncontrada_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterComDetalhesAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        var act = async () => await _sut.CancelarItemAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Ordem de Serviço*");
    }

    [Fact]
    public async Task CancelarItemAsync_ItemServico_DeveRemoverItem()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "");
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var item = new ItemServico(os.Id, Guid.NewGuid(), 1, 80m);
        os.AdicionarItemServico(item);

        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.CancelarItemAsync(os.Id, item.Id);

        resultado.ItensServico.Should().BeEmpty();
    }

    [Fact]
    public async Task CancelarItemAsync_ItemPeca_DeveRemoverItemERestaurarEstoque()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "");
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var peca = new Peca("Filtro", "Desc", 35m, 10);
        peca.ConsumirEstoque(3);
        var item = new ItemPeca(os.Id, peca.Id, 3, 35m);
        os.AdicionarItemPeca(item);

        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id)).ReturnsAsync(peca);
        _pecaRepoMock.Setup(r => r.AtualizarAsync(It.IsAny<Peca>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.CancelarItemAsync(os.Id, item.Id);

        resultado.ItensPeca.Should().BeEmpty();
        peca.QuantidadeEstoque.Should().Be(10);
    }

    [Fact]
    public async Task CancelarItemAsync_ItemNaoEncontrado_DeveLancarExcecao()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "");
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);

        var act = async () => await _sut.CancelarItemAsync(os.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Item*");
    }

    // ── AcompanharPorNumeroAsync ──────────────────────────────────────────────

    [Fact]
    public async Task AcompanharPorNumeroAsync_OrdemNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.ObterPorNumeroAsync(It.IsAny<string>())).ReturnsAsync((OrdemServico?)null);

        var resultado = await _sut.AcompanharPorNumeroAsync("OS-001");

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AcompanharPorNumeroAsync_OrdemExiste_DeveRetornarDto()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        _repoMock.Setup(r => r.ObterPorNumeroAsync("OS-001")).ReturnsAsync(os);

        var resultado = await _sut.AcompanharPorNumeroAsync("OS-001");

        resultado.Should().NotBeNull();
        resultado!.Numero.Should().Be("OS-001");
        resultado.Status.Should().Be(StatusOrdemServico.Recebida);
    }

    // ── ObterTempoIndividualAsync + FormatarTempo ─────────────────────────────

    private static OrdemServico CriarOsComDatas(DateTime abertura, DateTime? fechamento = null)
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "");
        typeof(OrdemServico).GetProperty("DataAbertura", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(os, abertura);
        if (fechamento.HasValue)
            typeof(OrdemServico).GetProperty("DataFechamento", BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(os, fechamento.Value);
        return os;
    }

    [Fact]
    public async Task ObterTempoIndividualAsync_OrdemNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.ObterComDetalhesPorNumeroAsync(It.IsAny<string>())).ReturnsAsync((OrdemServico?)null);

        var resultado = await _sut.ObterTempoIndividualAsync("OS-001");

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterTempoIndividualAsync_OsSemHistorico_DeveRetornarStatusAtualEObservacaoAndamento()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "");
        _repoMock.Setup(r => r.ObterComDetalhesPorNumeroAsync("OS-001")).ReturnsAsync(os);

        var resultado = await _sut.ObterTempoIndividualAsync("OS-001");

        resultado.Should().NotBeNull();
        resultado!.TemposPorStatus.Should().HaveCount(1);
        resultado.TemposPorStatus[0].Status.Should().Be(StatusOrdemServico.Recebida.ToString());
        resultado.Observacao.Should().Contain("andamento");
    }

    [Fact]
    public async Task ObterTempoIndividualAsync_OsFinalizadaComHistorico_DeveRetornarObservacaoFinalizada()
    {
        var abertura = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var fechamento = new DateTime(2026, 1, 1, 2, 0, 0, DateTimeKind.Utc);
        var os = CriarOsComDatas(abertura, fechamento);
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        _repoMock.Setup(r => r.ObterComDetalhesPorNumeroAsync("OS-001")).ReturnsAsync(os);

        var resultado = await _sut.ObterTempoIndividualAsync("OS-001");

        resultado.Should().NotBeNull();
        resultado!.Observacao.Should().Contain("finalizada");
        resultado.TemposPorStatus.Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [InlineData(0, 0, 30, "30min")]
    [InlineData(0, 2, 0,  "2h")]
    [InlineData(0, 2, 30, "2h 30min")]
    [InlineData(2, 0, 0,  "2 dia(s)")]
    [InlineData(2, 3, 0,  "2 dia(s) e 3h")]
    public async Task ObterTempoIndividualAsync_FormatarTempo_RetornaFormatoCorreto(
        int dias, int horas, int minutos, string esperado)
    {
        var abertura = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var fechamento = abertura.AddDays(dias).AddHours(horas).AddMinutes(minutos);
        var os = CriarOsComDatas(abertura, fechamento);
        _repoMock.Setup(r => r.ObterComDetalhesPorNumeroAsync("OS-001")).ReturnsAsync(os);

        var resultado = await _sut.ObterTempoIndividualAsync("OS-001");

        resultado!.TempoTotalFormatado.Should().Be(esperado);
    }
}
