using FluentAssertions;
using Moq;
using TechChallenge.Application.DTOs.Servico;
using TechChallenge.Application.Services;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using Xunit;

namespace TechChallenge.UnitTests.Application;

public class ServicoServiceTests
{
    private readonly Mock<IServicoRepository> _repoMock = new();
    private readonly ServicoService _sut;

    public ServicoServiceTests()
    {
        _sut = new ServicoService(_repoMock.Object);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaMapeada()
    {
        var servicos = new[] { new Servico("Troca de Óleo", "Desc", 80m, 30) };
        _repoMock.Setup(r => r.ObterTodosAsync()).ReturnsAsync(servicos);

        var resultado = await _sut.ObterTodosAsync();

        resultado.Should().HaveCount(1);
        resultado.First().Nome.Should().Be("Troca de Óleo");
    }

    [Fact]
    public async Task ObterPorIdAsync_ServicoExiste_DeveRetornarDto()
    {
        var id = Guid.NewGuid();
        var servico = new Servico("Alinhamento", "Desc", 120m, 60);
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(servico);

        var resultado = await _sut.ObterPorIdAsync(id);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Alinhamento");
        resultado.Preco.Should().Be(120m);
    }

    [Fact]
    public async Task ObterPorIdAsync_ServicoNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Servico?)null);

        var resultado = await _sut.ObterPorIdAsync(Guid.NewGuid());

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CriarAsync_DadosValidos_DeveCriarERetornarDto()
    {
        var dto = new CriarServicoDto { Nome = "Balanceamento", Descricao = "Desc", Preco = 100m, TempoConclusaoMinutos = 45 };
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Servico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.CriarAsync(dto);

        resultado.Nome.Should().Be("Balanceamento");
        resultado.Preco.Should().Be(100m);
        resultado.TempoConclusaoMinutos.Should().Be(45);
        resultado.Ativo.Should().BeTrue();
        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Servico>()), Times.Once);
        _repoMock.Verify(r => r.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_ServicoNaoEncontrado_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Servico?)null);

        var act = async () => await _sut.AtualizarAsync(Guid.NewGuid(), new CriarServicoDto());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Serviço não encontrado*");
    }

    [Fact]
    public async Task AtualizarAsync_ServicoExiste_DeveAtualizarERetornarDto()
    {
        var id = Guid.NewGuid();
        var servico = new Servico("Nome Antigo", "Desc Antiga", 50m, 20);
        var dto = new CriarServicoDto { Nome = "Nome Novo", Descricao = "Desc Nova", Preco = 75m, TempoConclusaoMinutos = 30 };
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(servico);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Servico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AtualizarAsync(id, dto);

        resultado.Nome.Should().Be("Nome Novo");
        resultado.Preco.Should().Be(75m);
        resultado.TempoConclusaoMinutos.Should().Be(30);
    }

    [Fact]
    public async Task DesativarAsync_ServicoNaoEncontrado_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Servico?)null);

        var act = async () => await _sut.DesativarAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DesativarAsync_ServicoExiste_DeveDesativar()
    {
        var id = Guid.NewGuid();
        var servico = new Servico("Troca de Óleo", "Desc", 80m, 30);
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(servico);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Servico>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        await _sut.DesativarAsync(id);

        servico.Ativo.Should().BeFalse();
        _repoMock.Verify(r => r.AtualizarAsync(servico), Times.Once);
    }
}
