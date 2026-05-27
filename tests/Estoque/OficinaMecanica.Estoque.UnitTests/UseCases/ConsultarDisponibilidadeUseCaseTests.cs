using Moq;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Application.UseCases;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.UnitTests.UseCases;

public class ConsultarDisponibilidadeUseCaseTests
{
    private readonly Mock<IPecaRepository> _pecaRepoMock = new();
    private readonly ConsultarDisponibilidadeUseCase _sut;

    public ConsultarDisponibilidadeUseCaseTests()
    {
        _sut = new ConsultarDisponibilidadeUseCase(_pecaRepoMock.Object);
    }

    [Fact]
    public async Task Retorna_True_QuandoTodasPecasTemEstoqueSuficiente()
    {
        var peca1 = new Peca("Filtro", "Filtro de óleo", 30m, 10);
        var peca2 = new Peca("Vela", "Vela de ignição", 15m, 5);

        _pecaRepoMock
            .Setup(r => r.ObterPorIdsAsync(It.IsAny<IEnumerable<Guid>>(), default))
            .ReturnsAsync([peca1, peca2]);

        var itens = new List<ItemDisponibilidade>
        {
            new(peca1.Id, 3),
            new(peca2.Id, 5)
        };

        var resultado = await _sut.ExecutarAsync(itens);

        Assert.True(resultado);
    }

    [Fact]
    public async Task Retorna_False_QuandoPeloMenosUmaPecaInsuficiente()
    {
        var peca1 = new Peca("Filtro", "Filtro de óleo", 30m, 10);
        var peca2 = new Peca("Vela", "Vela de ignição", 15m, 2);

        _pecaRepoMock
            .Setup(r => r.ObterPorIdsAsync(It.IsAny<IEnumerable<Guid>>(), default))
            .ReturnsAsync([peca1, peca2]);

        var itens = new List<ItemDisponibilidade>
        {
            new(peca1.Id, 3),
            new(peca2.Id, 5)
        };

        var resultado = await _sut.ExecutarAsync(itens);

        Assert.False(resultado);
    }
}
