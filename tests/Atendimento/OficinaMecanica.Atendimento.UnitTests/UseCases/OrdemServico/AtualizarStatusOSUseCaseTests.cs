using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OficinaMecanica.Atendimento.Application.Events;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;
using OficinaMecanica.Atendimento.Domain.Enums;
using DomainOS = OficinaMecanica.Atendimento.Domain.Entities.OrdemServico;
using DomainCliente = OficinaMecanica.Atendimento.Domain.Entities.Cliente;

namespace OficinaMecanica.Atendimento.UnitTests.UseCases.OS;

public class AtualizarStatusOSUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repoMock = new();
    private readonly Mock<IEventPublisher> _eventMock = new();
    private readonly Mock<IEmailPort> _emailMock = new();
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly AtualizarStatusOSUseCase _sut;

    public AtualizarStatusOSUseCaseTests()
    {
        _sut = new AtualizarStatusOSUseCase(_repoMock.Object, _eventMock.Object, _emailMock.Object, _clienteRepoMock.Object, NullLogger<AtualizarStatusOSUseCase>.Instance);
    }

    private static DomainOS CriarOS()
        => new("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");

    [Fact]
    public async Task ExecutarAsync_StatusValido_AvancaStatus()
    {
        var os = CriarOS();
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id, default)).ReturnsAsync(os);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(os.ClienteId, default)).ReturnsAsync((DomainCliente?)null);

        await _sut.ExecutarAsync(os.Id, StatusOrdemServico.EmDiagnostico);

        Assert.Equal(StatusOrdemServico.EmDiagnostico, os.Status);
    }

    [Fact]
    public async Task ExecutarAsync_StatusInvalido_LancaInvalidOperationException()
    {
        var os = CriarOS();
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id, default)).ReturnsAsync(os);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ExecutarAsync(os.Id, StatusOrdemServico.Finalizada));
    }

    [Fact]
    public async Task ExecutarAsync_StatusFinalizada_PublicaEvento()
    {
        var os = CriarOS();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);
        os.AlterarStatus(StatusOrdemServico.EmExecucao);
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id, default)).ReturnsAsync(os);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(os.ClienteId, default)).ReturnsAsync((DomainCliente?)null);

        await _sut.ExecutarAsync(os.Id, StatusOrdemServico.Finalizada);

        _eventMock.Verify(e => e.PublishOsFinalizadaAsync(It.IsAny<OsFinalizadaEvent>(), default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteExiste_EnviaEmail()
    {
        var clienteId = Guid.NewGuid();
        var os = new DomainOS("OS-001", clienteId, Guid.NewGuid(), "Obs");
        var cliente = new DomainCliente("João", "12345678909", "joao@email.com", "11999999999", "Rua A");
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id, default)).ReturnsAsync(os);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId, default)).ReturnsAsync(cliente);

        await _sut.ExecutarAsync(os.Id, StatusOrdemServico.EmDiagnostico);

        _emailMock.Verify(e => e.EnviarAtualizacaoStatusAsync(
            cliente.Email, os.Numero, StatusOrdemServico.EmDiagnostico, default), Times.Once);
    }
}
