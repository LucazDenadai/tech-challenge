using Moq;
using OficinaMecanica.Estoque.Application.Exceptions;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Application.UseCases;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.UnitTests.UseCases;

public class GerenciarPecaUseCaseTests
{
    private readonly Mock<IPecaRepository> _pecaRepoMock = new();
    private readonly GerenciarPecaUseCase _sut;

    public GerenciarPecaUseCaseTests()
    {
        _sut = new GerenciarPecaUseCase(_pecaRepoMock.Object);
    }

    [Fact]
    public async Task CriarPeca_RetornaPecaCriada()
    {
        var request = new CriarPecaRequest("Filtro", "Filtro de óleo", 30m, 10);

        var response = await _sut.CriarAsync(request);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Filtro", response.Nome);
        _pecaRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Peca>(), default), Times.Once);
        _pecaRepoMock.Verify(r => r.SalvarAsync(default), Times.Once);
    }

    [Fact]
    public async Task ObterPecaPorId_Retorna_QuandoExiste()
    {
        var peca = new Peca("Filtro", "Filtro de óleo", 30m, 10);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id, default)).ReturnsAsync(peca);

        var response = await _sut.ObterPorIdAsync(peca.Id);

        Assert.NotNull(response);
        Assert.Equal(peca.Id, response.Id);
    }

    [Fact]
    public async Task ObterPecaPorId_Retorna_Null_QuandoNaoExiste()
    {
        var id = Guid.NewGuid();
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(id, default)).ReturnsAsync((Peca?)null);

        var response = await _sut.ObterPorIdAsync(id);

        Assert.Null(response);
    }

    [Fact]
    public async Task AtualizarPeca_PersisteMudancas()
    {
        var peca = new Peca("Filtro", "Filtro de óleo", 30m, 10);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id, default)).ReturnsAsync(peca);

        var request = new AtualizarPecaRequest(peca.Id, "Filtro Premium", "Filtro sintético", 50m);
        await _sut.AtualizarAsync(request);

        Assert.Equal("Filtro Premium", peca.Nome);
        Assert.Equal(50m, peca.Valor);
        _pecaRepoMock.Verify(r => r.AtualizarAsync(peca, default), Times.Once);
        _pecaRepoMock.Verify(r => r.SalvarAsync(default), Times.Once);
    }

    [Fact]
    public async Task AtualizarPeca_LancaNotFoundException_QuandoNaoExiste()
    {
        var id = Guid.NewGuid();
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(id, default)).ReturnsAsync((Peca?)null);

        var request = new AtualizarPecaRequest(id, "X", "Y", 10m);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AtualizarAsync(request));
    }

    [Fact]
    public async Task RemoverPeca_ChamaRepositorio()
    {
        var peca = new Peca("Filtro", "Filtro de óleo", 30m, 10);
        _pecaRepoMock.Setup(r => r.ObterPorIdAsync(peca.Id, default)).ReturnsAsync(peca);

        await _sut.RemoverAsync(peca.Id);

        _pecaRepoMock.Verify(r => r.RemoverAsync(peca, default), Times.Once);
        _pecaRepoMock.Verify(r => r.SalvarAsync(default), Times.Once);
    }
}
