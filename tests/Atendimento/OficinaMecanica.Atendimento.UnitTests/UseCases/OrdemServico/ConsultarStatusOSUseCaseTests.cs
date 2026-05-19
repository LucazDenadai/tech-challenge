using Moq;
using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;
using OficinaMecanica.Atendimento.Domain.Enums;
using DomainOS = OficinaMecanica.Atendimento.Domain.Entities.OrdemServico;

namespace OficinaMecanica.Atendimento.UnitTests.UseCases.OS;

public class ConsultarStatusOSUseCaseTests
{
    private readonly Mock<IOrdemServicoRepository> _repoMock = new();
    private readonly ConsultarStatusOSUseCase _sut;

    public ConsultarStatusOSUseCaseTests()
    {
        _sut = new ConsultarStatusOSUseCase(_repoMock.Object);
    }

    [Fact]
    public async Task ExecutarAsync_OSExiste_RetornaStatusEHistorico()
    {
        var os = new DomainOS("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Obs");
        _repoMock.Setup(r => r.ObterComDetalhesAsync(os.Id, default)).ReturnsAsync(os);

        var result = await _sut.ExecutarAsync(os.Id);

        Assert.Equal(os.Id, result.Id);
        Assert.Equal("OS-001", result.Numero);
        Assert.Equal(StatusOrdemServico.Recebida, result.Status);
    }

    [Fact]
    public async Task ExecutarAsync_OSNaoEncontrada_LancaNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.ObterComDetalhesAsync(id, default)).ReturnsAsync((DomainOS?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ExecutarAsync(id));
    }
}
