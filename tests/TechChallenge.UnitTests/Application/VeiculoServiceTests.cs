using FluentAssertions;
using Moq;
using TechChallenge.Application.DTOs.Veiculo;
using TechChallenge.Application.Services;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using Xunit;

namespace TechChallenge.UnitTests.Application;

public class VeiculoServiceTests
{
    private readonly Mock<IVeiculoRepository> _repoMock = new();
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly VeiculoService _sut;

    public VeiculoServiceTests()
    {
        _sut = new VeiculoService(_repoMock.Object, _clienteRepoMock.Object);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaMapeada()
    {
        var clienteId = Guid.NewGuid();
        var veiculos = new[] { new Veiculo(clienteId, "ABC1234", "Toyota", "Corolla", 2020, "Prata") };
        _repoMock.Setup(r => r.ObterTodosAsync()).ReturnsAsync(veiculos);

        var resultado = await _sut.ObterTodosAsync();

        resultado.Should().HaveCount(1);
        resultado.First().Placa.Should().Be("ABC1234");
    }

    [Fact]
    public async Task ObterPorIdAsync_VeiculoExiste_DeveRetornarDto()
    {
        var id = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var veiculo = new Veiculo(clienteId, "DEF5678", "Honda", "Civic", 2019, "Preto");
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(veiculo);

        var resultado = await _sut.ObterPorIdAsync(id);

        resultado.Should().NotBeNull();
        resultado!.Placa.Should().Be("DEF5678");
        resultado.Marca.Should().Be("Honda");
    }

    [Fact]
    public async Task ObterPorIdAsync_VeiculoNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Veiculo?)null);

        var resultado = await _sut.ObterPorIdAsync(Guid.NewGuid());

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterPorClienteAsync_DeveRetornarVeiculosDoCliente()
    {
        var clienteId = Guid.NewGuid();
        var veiculos = new[]
        {
            new Veiculo(clienteId, "ABC1234", "Toyota", "Corolla", 2020, "Prata"),
            new Veiculo(clienteId, "DEF5678", "Honda", "Civic", 2019, "Preto"),
        };
        _repoMock.Setup(r => r.ObterPorClienteAsync(clienteId)).ReturnsAsync(veiculos);

        var resultado = await _sut.ObterPorClienteAsync(clienteId);

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task CriarAsync_ClienteNaoEncontrado_DeveLancarExcecao()
    {
        var dto = new CriarVeiculoDto { ClienteId = Guid.NewGuid(), Placa = "ABC1234", Marca = "Toyota", Modelo = "Corolla", Ano = 2020, Cor = "Prata" };
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(dto.ClienteId)).ReturnsAsync((Cliente?)null);

        var act = async () => await _sut.CriarAsync(dto);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Cliente não encontrado*");
    }

    [Fact]
    public async Task CriarAsync_ClienteExiste_DeveCriarERetornarDto()
    {
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("Carlos Silva", "52998224725", "carlos@email.com", "11987654321", "Rua das Flores");
        var dto = new CriarVeiculoDto { ClienteId = clienteId, Placa = "GHI9012", Marca = "VW", Modelo = "Golf", Ano = 2022, Cor = "Branco" };
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId)).ReturnsAsync(cliente);
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Veiculo>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.CriarAsync(dto);

        resultado.Placa.Should().Be("GHI9012");
        resultado.Marca.Should().Be("VW");
        resultado.Ano.Should().Be(2022);
        resultado.NomeCliente.Should().Be("Carlos Silva");
        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Veiculo>()), Times.Once);
        _repoMock.Verify(r => r.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_VeiculoNaoEncontrado_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Veiculo?)null);

        var act = async () => await _sut.AtualizarAsync(Guid.NewGuid(), new CriarVeiculoDto());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Veículo não encontrado*");
    }

    [Fact]
    public async Task AtualizarAsync_VeiculoExiste_DeveAtualizarERetornarDto()
    {
        var id = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var veiculo = new Veiculo(clienteId, "ABC1234", "Toyota", "Corolla", 2020, "Prata");
        var dto = new CriarVeiculoDto { ClienteId = clienteId, Placa = "ABC1234", Marca = "Toyota", Modelo = "Corolla", Ano = 2021, Cor = "Azul" };
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Veiculo>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AtualizarAsync(id, dto);

        resultado.Ano.Should().Be(2021);
        resultado.Cor.Should().Be("Azul");
    }

    [Fact]
    public async Task RemoverAsync_VeiculoNaoEncontrado_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Veiculo?)null);

        var act = async () => await _sut.RemoverAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Veículo não encontrado*");
    }

    [Fact]
    public async Task RemoverAsync_VeiculoExiste_DeveRemover()
    {
        var id = Guid.NewGuid();
        var veiculo = new Veiculo(Guid.NewGuid(), "ABC1234", "Toyota", "Corolla", 2020, "Prata");
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.RemoverAsync(It.IsAny<Veiculo>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        await _sut.RemoverAsync(id);

        _repoMock.Verify(r => r.RemoverAsync(veiculo), Times.Once);
        _repoMock.Verify(r => r.SalvarAsync(), Times.Once);
    }
}
