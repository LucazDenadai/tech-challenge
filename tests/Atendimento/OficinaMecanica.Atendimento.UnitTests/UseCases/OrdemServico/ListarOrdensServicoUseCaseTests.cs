using Moq;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;
using OficinaMecanica.Atendimento.Domain.Enums;
using DomainOS = OficinaMecanica.Atendimento.Domain.Entities.OrdemServico;

namespace OficinaMecanica.Atendimento.UnitTests.UseCases.OS;

public class ListarOrdensServicoUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repoMock = new();
    private readonly ListarOrdensServicoUseCase _sut;

    public ListarOrdensServicoUseCaseTests()
    {
        _sut = new ListarOrdensServicoUseCase(_repoMock.Object);
    }

    private static DomainOS CriarOSComStatus(string numero, StatusOrdemServico status)
    {
        var os = new DomainOS(numero, Guid.NewGuid(), Guid.NewGuid(), "");
        var sequencia = new[]
        {
            StatusOrdemServico.EmDiagnostico,
            StatusOrdemServico.AguardandoAprovacao,
            StatusOrdemServico.EmExecucao,
            StatusOrdemServico.Finalizada,
            StatusOrdemServico.Entregue,
        };
        foreach (var s in sequencia)
        {
            if (os.Status == status) break;
            os.AlterarStatus(s);
        }
        return os;
    }

    [Fact]
    public async Task ExecutarAsync_RetornaOSNaOrdemCorretaDePrioridade()
    {
        var osRecebida = new DomainOS("OS-001", Guid.NewGuid(), Guid.NewGuid(), "");
        var osEmDiagnostico = CriarOSComStatus("OS-002", StatusOrdemServico.EmDiagnostico);
        var osEmExecucao = CriarOSComStatus("OS-003", StatusOrdemServico.EmExecucao);
        var osAguardando = CriarOSComStatus("OS-004", StatusOrdemServico.AguardandoAprovacao);

        _repoMock.Setup(r => r.ObterTodosAsync(default))
            .ReturnsAsync([osRecebida, osEmDiagnostico, osEmExecucao, osAguardando]);

        var result = (await _sut.ExecutarAsync()).ToList();

        Assert.Equal(4, result.Count);
        Assert.Equal(StatusOrdemServico.EmExecucao, result[0].Status);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, result[1].Status);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, result[2].Status);
        Assert.Equal(StatusOrdemServico.Recebida, result[3].Status);
    }

    [Fact]
    public async Task ExecutarAsync_NaoRetornaFinalizadaNemEntregue()
    {
        var osFinalizada = CriarOSComStatus("OS-001", StatusOrdemServico.Finalizada);
        var osEntregue = CriarOSComStatus("OS-002", StatusOrdemServico.Entregue);
        _repoMock.Setup(r => r.ObterTodosAsync(default)).ReturnsAsync([osFinalizada, osEntregue]);

        var result = await _sut.ExecutarAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecutarAsync_MesmoStatus_OrdenaPorDataAberturaMaisAntiga()
    {
        var os1 = new DomainOS("OS-001", Guid.NewGuid(), Guid.NewGuid(), "");
        await Task.Delay(10);
        var os2 = new DomainOS("OS-002", Guid.NewGuid(), Guid.NewGuid(), "");

        _repoMock.Setup(r => r.ObterTodosAsync(default)).ReturnsAsync([os2, os1]);

        var result = (await _sut.ExecutarAsync()).ToList();

        Assert.Equal("OS-001", result[0].Numero);
        Assert.Equal("OS-002", result[1].Numero);
    }
}
