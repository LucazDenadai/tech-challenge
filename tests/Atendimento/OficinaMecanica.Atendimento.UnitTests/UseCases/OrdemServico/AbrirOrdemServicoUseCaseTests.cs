using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;
using DomainOS = OficinaMecanica.Atendimento.Domain.Entities.OrdemServico;
using DomainVeiculo = OficinaMecanica.Atendimento.Domain.Entities.Veiculo;

namespace OficinaMecanica.Atendimento.UnitTests.UseCases.OS;

public class AbrirOrdemServicoUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _osRepoMock = new();
    private readonly Mock<IVeiculoRepository> _veiculoRepoMock = new();
    private readonly Mock<IEstoquePort> _estoqueMock = new();
    private readonly AbrirOrdemServicoUseCase _sut;

    private readonly Guid _clienteId = Guid.NewGuid();
    private readonly Guid _veiculoId = Guid.NewGuid();

    public AbrirOrdemServicoUseCaseTests()
    {
        _sut = new AbrirOrdemServicoUseCase(_osRepoMock.Object, _veiculoRepoMock.Object, _estoqueMock.Object, NullLogger<AbrirOrdemServicoUseCase>.Instance);
    }

    [Fact]
    public async Task ExecutarAsync_DadosValidos_CriaOSComStatusRecebida()
    {
        var veiculo = new DomainVeiculo(_clienteId, "ABC1D23", "Ford", "Fiesta", 2020, "Prata");
        _veiculoRepoMock.Setup(r => r.ObterPorIdAsync(_veiculoId, default)).ReturnsAsync(veiculo);
        _osRepoMock.Setup(r => r.GerarNumeroAsync(default)).ReturnsAsync("OS-001");

        var request = new AbrirOrdemServicoRequest(_clienteId, _veiculoId, "Revisão", []);

        var result = await _sut.ExecutarAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("OS-001", result.Numero);
        _osRepoMock.Verify(r => r.AdicionarAsync(
            It.Is<DomainOS>(os => os.Status == OficinaMecanica.Atendimento.Domain.Enums.StatusOrdemServico.Recebida),
            default), Times.Once);
        _osRepoMock.Verify(r => r.SalvarAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_PecasIndisponiveis_LancaInvalidOperationException()
    {
        var veiculo = new DomainVeiculo(_clienteId, "ABC1D23", "Ford", "Fiesta", 2020, "Prata");
        _veiculoRepoMock.Setup(r => r.ObterPorIdAsync(_veiculoId, default)).ReturnsAsync(veiculo);
        _estoqueMock.Setup(e => e.VerificarDisponibilidadeAsync(It.IsAny<IEnumerable<ItemPecaRequest>>(), default))
            .ReturnsAsync(false);

        var pecas = new List<ItemPecaRequest> { new(Guid.NewGuid(), 2) };
        var request = new AbrirOrdemServicoRequest(_clienteId, _veiculoId, "Troca de peças", pecas);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecutarAsync(request));
    }

    [Fact]
    public async Task ExecutarAsync_VeiculoDeOutroCliente_LancaInvalidOperationException()
    {
        var outroClienteId = Guid.NewGuid();
        var veiculo = new DomainVeiculo(outroClienteId, "XYZ9A10", "Honda", "Civic", 2022, "Preto");
        _veiculoRepoMock.Setup(r => r.ObterPorIdAsync(_veiculoId, default)).ReturnsAsync(veiculo);

        var request = new AbrirOrdemServicoRequest(_clienteId, _veiculoId, "Obs", []);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecutarAsync(request));
    }
}
