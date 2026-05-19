using Moq;
using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;
using OficinaMecanica.Atendimento.Domain.Enums;
using DomainOS = OficinaMecanica.Atendimento.Domain.Entities.OrdemServico;

namespace OficinaMecanica.Atendimento.UnitTests.UseCases.OS;

public class AprovarOrcamentoUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repoMock = new();
    private readonly AprovarOrcamentoUseCase _sut;

    public AprovarOrcamentoUseCaseTests()
    {
        _sut = new AprovarOrcamentoUseCase(_repoMock.Object);
    }

    private static DomainOS CriarOSEmAguardandoAprovacao()
    {
        var os = new DomainOS("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);
        return os;
    }

    [Fact]
    public async Task ExecutarAsync_Aprovado_MudaStatusParaEmExecucao()
    {
        var os = CriarOSEmAguardandoAprovacao();
        _repoMock.Setup(r => r.ObterPorIdAsync(os.Id, default)).ReturnsAsync(os);

        await _sut.ExecutarAsync(os.Id, aprovado: true);

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
        _repoMock.Verify(r => r.SalvarAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_Recusado_MudaStatusParaCancelada()
    {
        var os = CriarOSEmAguardandoAprovacao();
        _repoMock.Setup(r => r.ObterPorIdAsync(os.Id, default)).ReturnsAsync(os);

        await _sut.ExecutarAsync(os.Id, aprovado: false);

        Assert.Equal(StatusOrdemServico.Cancelada, os.Status);
    }

    [Fact]
    public async Task ExecutarAsync_OSNaoEmAguardandoAprovacao_LancaInvalidOperationException()
    {
        var os = new DomainOS("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        _repoMock.Setup(r => r.ObterPorIdAsync(os.Id, default)).ReturnsAsync(os);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecutarAsync(os.Id, aprovado: true));
    }

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_LancaNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.ObterPorIdAsync(id, default)).ReturnsAsync((DomainOS?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ExecutarAsync(id, aprovado: true));
    }
}
