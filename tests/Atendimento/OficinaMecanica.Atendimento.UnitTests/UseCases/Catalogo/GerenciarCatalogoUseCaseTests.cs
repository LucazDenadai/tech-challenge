using Moq;
using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Application.UseCases.Catalogo;
using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.UnitTests.UseCases.Catalogo;

public class GerenciarCatalogoUseCaseTests
{
    private readonly Mock<IServicoRepository> _servicoRepoMock = new();
    private readonly GerenciarCatalogoUseCase _sut;

    public GerenciarCatalogoUseCaseTests()
    {
        _sut = new GerenciarCatalogoUseCase(_servicoRepoMock.Object);
    }

    [Fact]
    public async Task CriarServicoAsync_DadosValidos_RetornaId()
    {
        var request = new CriarServicoRequest("Troca de óleo", "Troca completa", 150m, 60);

        var response = await _sut.CriarServicoAsync(request);

        Assert.NotEqual(Guid.Empty, response.Id);
        _servicoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Servico>(), default), Times.Once);
        _servicoRepoMock.Verify(r => r.SalvarAsync(default), Times.Once);
    }

    [Fact]
    public async Task AtualizarServicoAsync_ServicoExiste_AtualizaPreco()
    {
        var servico = new Servico("Alinhamento", "Alinhamento de rodas", 100m, 30);
        _servicoRepoMock.Setup(r => r.ObterPorIdAsync(servico.Id, default)).ReturnsAsync(servico);

        var request = new AtualizarServicoRequest(servico.Id, "Alinhamento", "Alinhamento e balanceamento", 120m, 45);
        await _sut.AtualizarServicoAsync(request);

        Assert.Equal(120m, servico.Preco);
        _servicoRepoMock.Verify(r => r.SalvarAsync(default), Times.Once);
    }

    [Fact]
    public async Task AtualizarServicoAsync_ServicoNaoEncontrado_LancaNotFoundException()
    {
        var id = Guid.NewGuid();
        _servicoRepoMock.Setup(r => r.ObterPorIdAsync(id, default)).ReturnsAsync((Servico?)null);

        var request = new AtualizarServicoRequest(id, "X", "Y", 10m, 10);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AtualizarServicoAsync(request));
    }
}
