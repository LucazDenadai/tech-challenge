using FluentAssertions;
using Moq;
using TechChallenge.Application.DTOs.Peca;
using TechChallenge.Application.Services;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using Xunit;

namespace TechChallenge.UnitTests.Application;

public class PecaServiceTests
{
    private readonly Mock<IPecaRepository> _repoMock = new();
    private readonly PecaService _sut;

    public PecaServiceTests()
    {
        _sut = new PecaService(_repoMock.Object);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaMapeada()
    {
        var pecas = new[] { new Peca("Filtro", "Desc", 35m, 10) };
        _repoMock.Setup(r => r.ObterAtivosAsync()).ReturnsAsync(pecas);

        var resultado = await _sut.ObterTodosAsync();

        resultado.Should().HaveCount(1);
        resultado.First().Nome.Should().Be("Filtro");
    }

    [Fact]
    public async Task ObterPorIdAsync_PecaExiste_DeveRetornarDto()
    {
        var id = Guid.NewGuid();
        var peca = new Peca("Filtro", "Desc", 35m, 10);
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(peca);

        var resultado = await _sut.ObterPorIdAsync(id);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Filtro");
    }

    [Fact]
    public async Task CriarAsync_DadosValidos_DeveCriarERetornarDto()
    {
        var dto = new CriarPecaDto { Nome = "Filtro", Descricao = "Desc", Preco = 35m, QuantidadeEstoque = 10 };
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Peca>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.CriarAsync(dto);

        resultado.Nome.Should().Be("Filtro");
        resultado.Preco.Should().Be(35m);
        resultado.QuantidadeEstoque.Should().Be(10);
    }

    [Fact]
    public async Task AtualizarAsync_PecaNaoEncontrada_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Peca?)null);

        var act = async () => await _sut.AtualizarAsync(Guid.NewGuid(), new CriarPecaDto());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Peça não encontrada*");
    }

    [Fact]
    public async Task AtualizarAsync_ComEstoquePositivo_DeveAdicionarEstoque()
    {
        var id = Guid.NewGuid();
        var peca = new Peca("Filtro", "Desc", 35m, 10);
        var dto = new CriarPecaDto { Nome = "Filtro Novo", Descricao = "Nova Desc", Preco = 40m, QuantidadeEstoque = 5 };
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(peca);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Peca>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AtualizarAsync(id, dto);

        resultado.QuantidadeEstoque.Should().Be(15); // 10 original + 5 adicionado
        resultado.Preco.Should().Be(40m);
    }

    [Fact]
    public async Task DesativarAsync_PecaNaoEncontrada_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Peca?)null);

        var act = async () => await _sut.DesativarAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DesativarAsync_PecaExiste_DeveDesativar()
    {
        var id = Guid.NewGuid();
        var peca = new Peca("Filtro", "Desc", 35m, 10);
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(peca);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Peca>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        await _sut.DesativarAsync(id);

        peca.Ativo.Should().BeFalse();
    }
}
