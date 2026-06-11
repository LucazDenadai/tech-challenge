using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Application.UseCases;
using OficinaMecanica.Estoque.Domain.Entities;
using OficinaMecanica.Estoque.Domain.Enums;

namespace OficinaMecanica.Estoque.UnitTests.UseCases;

public class BaixarEstoqueUseCaseTests
{
    private readonly Mock<IPecaRepository> _pecaRepoMock = new();
    private readonly Mock<IMovimentacaoRepository> _movRepoMock = new();
    private readonly BaixarEstoqueUseCase _sut;

    public BaixarEstoqueUseCaseTests()
    {
        _sut = new BaixarEstoqueUseCase(_pecaRepoMock.Object, _movRepoMock.Object, NullLogger<BaixarEstoqueUseCase>.Instance);
    }

    [Fact]
    public async Task SubtraiCorretamente_ERegistraMovimentacao()
    {
        var peca = new Peca("Filtro", "Filtro de óleo", 30m, 10);
        var osId = Guid.NewGuid();

        _movRepoMock.Setup(r => r.ExisteMovimentacaoPorOsIdAsync(osId, default)).ReturnsAsync(false);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id, default)).ReturnsAsync(peca);

        var itens = new List<ItemBaixa> { new(peca.Id, 3) };
        await _sut.ExecutarAsync(osId, itens);

        Assert.Equal(7, peca.QuantidadeEstoque);
        _pecaRepoMock.Verify(r => r.AtualizarAsync(peca, default), Times.Once);
        _movRepoMock.Verify(r => r.AdicionarAsync(
            It.Is<MovimentacaoEstoque>(m => m.OsId == osId && m.Tipo == TipoMovimentacao.Saida && m.Quantidade == 3),
            default), Times.Once);
        _movRepoMock.Verify(r => r.SalvarAsync(default), Times.Once);
    }

    [Fact]
    public async Task Lanca_Excecao_QuandoQuantidadeSolicitadaMaiorQueEstoque()
    {
        var peca = new Peca("Filtro", "Filtro de óleo", 30m, 2);
        var osId = Guid.NewGuid();

        _movRepoMock.Setup(r => r.ExisteMovimentacaoPorOsIdAsync(osId, default)).ReturnsAsync(false);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id, default)).ReturnsAsync(peca);

        var itens = new List<ItemBaixa> { new(peca.Id, 5) };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecutarAsync(osId, itens));
    }

    [Fact]
    public async Task Idempotente_QuandoOsIdJaProcessado()
    {
        var osId = Guid.NewGuid();
        _movRepoMock.Setup(r => r.ExisteMovimentacaoPorOsIdAsync(osId, default)).ReturnsAsync(true);

        var itens = new List<ItemBaixa> { new(Guid.NewGuid(), 3) };
        await _sut.ExecutarAsync(osId, itens);

        _pecaRepoMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), default), Times.Never);
        _movRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<MovimentacaoEstoque>(), default), Times.Never);
    }
}
