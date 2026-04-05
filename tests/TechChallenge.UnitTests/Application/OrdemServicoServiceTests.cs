using FluentAssertions;
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
    private readonly OrdemServicoService _sut;

    public OrdemServicoServiceTests()
    {
        _sut = new OrdemServicoService(_repoMock.Object, _servicoRepoMock.Object, _pecaRepoMock.Object);
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
        var dto = new CriarOrdemServicoDto
        {
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            Observacoes = "Revisão geral"
        };
        _repoMock.Setup(r => r.GerarNumeroAsync()).ReturnsAsync("OS-2024-001");
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.CriarAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Numero.Should().Be("OS-2024-001");
        resultado.ClienteId.Should().Be(dto.ClienteId);
    }

    [Fact]
    public async Task AvancarStatusAsync_OrdemNaoEncontrada_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterComDetalhesAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        var act = async () => await _sut.AvancarStatusAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Ordem de Serviço não encontrada*");
    }

    [Fact]
    public async Task AvancarStatusAsync_OrdemExiste_DeveAvancarStatus()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AvancarStatusAsync(os.Id);
        resultado.Status.Should().Be(StatusOrdemServico.EmDiagnostico);
        resultado.Status.Should().Be(StatusOrdemServico.EmDiagnostico);
    }

    [Fact]
    public async Task AdicionarItemServicoAsync_OrdemNaoEncontrada_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterComDetalhesAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        var act = async () => await _sut.AdicionarItemServicoAsync(Guid.NewGuid(), new AdicionarItemServicoDto());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Ordem de Serviço não encontrada*");
    }

    [Fact]
    public async Task AdicionarItemServicoAsync_ServicoNaoEncontrado_DeveLancarExcecao()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        var dto = new AdicionarItemServicoDto { ServicoId = Guid.NewGuid(), Quantidade = 1 };
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _servicoRepoMock.Setup(r => r.ObterPorIdAsync(dto.ServicoId)).ReturnsAsync((Servico?)null);

        var act = async () => await _sut.AdicionarItemServicoAsync(os.Id, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Serviço não encontrado*");
    }

    [Fact]
    public async Task AdicionarItemServicoAsync_DadosValidos_DeveAdicionarItem()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        var servico = new Servico("Troca de Óleo", "Desc", 80m, 30);
        var dto = new AdicionarItemServicoDto { ServicoId = servico.Id, Quantidade = 2 };
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _servicoRepoMock.Setup(r => r.ObterPorIdAsync(servico.Id)).ReturnsAsync(servico);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AdicionarItemServicoAsync(os.Id, dto);

        resultado.ItensServico.Should().HaveCount(1);
        resultado.ValorTotal.Should().Be(160m); // 2 * 80
    }

    [Fact]
    public async Task AdicionarItemPecaAsync_EstoqueInsuficiente_DeveLancarExcecao()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        var peca = new Peca("Filtro", "Desc", 35m, 1);
        var dto = new AdicionarItemPecaDto { PecaId = peca.Id, Quantidade = 5 };
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id)).ReturnsAsync(peca);

        var act = async () => await _sut.AdicionarItemPecaAsync(os.Id, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Estoque insuficiente*");
    }

    [Fact]
    public async Task AdicionarItemPecaAsync_DadosValidos_DeveAdicionarEConsumirEstoque()
    {
        var os = new OrdemServico("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        var peca = new Peca("Filtro", "Desc", 35m, 10);
        var dto = new AdicionarItemPecaDto { PecaId = peca.Id, Quantidade = 3 };
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id)).ReturnsAsync(os);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id)).ReturnsAsync(peca);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        _pecaRepoMock.Setup(r => r.AtualizarAsync(It.IsAny<Peca>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AdicionarItemPecaAsync(os.Id, dto);

        resultado.ItensPeca.Should().HaveCount(1);
        peca.QuantidadeEstoque.Should().Be(7); // 10 - 3
    }
}
