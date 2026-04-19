using FluentAssertions;
using Moq;
using TechChallenge.Application.DTOs.Cliente;
using TechChallenge.Application.Services;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using Xunit;

namespace TechChallenge.UnitTests.Application;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _repoMock = new();
    private readonly ClienteService _sut;

    public ClienteServiceTests()
    {
        _sut = new ClienteService(_repoMock.Object);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaMapeada()
    {
        IEnumerable<(Cliente Cliente, int TotalOrdens)> clientes = new[]
        {
            (new Cliente("João", "52998224725", "joao@email.com", "11999999999", "Rua A"), 2),
            (new Cliente("Maria", "11144477735", "maria@email.com", "11888888888", "Rua B"), 0),
        };
        _repoMock.Setup(r => r.ObterTodosComDetalhesAsync()).ReturnsAsync(clientes);

        var resultado = await _sut.ObterTodosAsync();

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterPorIdAsync_ClienteExiste_DeveRetornarDto()
    {
        var id = Guid.NewGuid();
        var cliente = new Cliente("João", "52998224725", "joao@email.com", "11999999999", "Rua A");
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(cliente);

        var resultado = await _sut.ObterPorIdAsync(id);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("João");
    }

    [Fact]
    public async Task ObterPorIdAsync_ClienteNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

        var resultado = await _sut.ObterPorIdAsync(Guid.NewGuid());

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CriarAsync_DocumentoJaCadastrado_DeveLancarExcecao()
    {
        var dto = new CriarClienteDto { Nome = "João", Documento = "52998224725", Email = "j@e.com", Telefone = "11999", Endereco = "Rua" };
        _repoMock.Setup(r => r.DocumentoExisteAsync(dto.Documento, null)).ReturnsAsync(true);

        var act = async () => await _sut.CriarAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CPF/CNPJ já cadastrado*");
    }

    [Fact]
    public async Task CriarAsync_DadosValidos_DeveCriarERetornarDto()
    {
        var dto = new CriarClienteDto { Nome = "João", Documento = "52998224725", Email = "j@e.com", Telefone = "11999", Endereco = "Rua" };
        _repoMock.Setup(r => r.DocumentoExisteAsync(dto.Documento, null)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.CriarAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("João");
        resultado.Documento.Should().Be("52998224725");
        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Cliente>()), Times.Once);
        _repoMock.Verify(r => r.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_ClienteNaoEncontrado_DeveLancarExcecao()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Cliente?)null);

        var act = async () => await _sut.AtualizarAsync(id, new CriarClienteDto());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Cliente não encontrado*");
    }

    [Fact]
    public async Task AtualizarAsync_DocumentoDuplicado_DeveLancarExcecao()
    {
        var id = Guid.NewGuid();
        var cliente = new Cliente("João", "52998224725", "j@e.com", "11999", "Rua");
        var dto = new CriarClienteDto { Nome = "João 2", Documento = "11144477735", Email = "j2@e.com", Telefone = "11888", Endereco = "Rua 2" };
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(cliente);
        _repoMock.Setup(r => r.DocumentoExisteAsync(dto.Documento, id)).ReturnsAsync(true);

        var act = async () => await _sut.AtualizarAsync(id, dto);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AtualizarAsync_DadosValidos_DeveAtualizarERetornarDto()
    {
        var id = Guid.NewGuid();
        var cliente = new Cliente("João", "52998224725", "j@e.com", "11999", "Rua A");
        var dto = new CriarClienteDto { Nome = "João Atualizado", Documento = "52998224725", Email = "j2@e.com", Telefone = "11888", Endereco = "Rua B" };
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(cliente);
        _repoMock.Setup(r => r.DocumentoExisteAsync(dto.Documento, id)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AtualizarAsync(id, dto);

        resultado.Nome.Should().Be("João Atualizado");
        resultado.Email.Should().Be("j2@e.com");
        _repoMock.Verify(r => r.AtualizarAsync(cliente), Times.Once);
        _repoMock.Verify(r => r.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task DesativarAsync_ClienteNaoEncontrado_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

        var act = async () => await _sut.DesativarAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DesativarAsync_ClienteExiste_DeveDesativar()
    {
        var id = Guid.NewGuid();
        var cliente = new Cliente("João", "52998224725", "j@e.com", "11999", "Rua");
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(cliente);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        await _sut.DesativarAsync(id);

        cliente.Ativo.Should().BeFalse();
        _repoMock.Verify(r => r.AtualizarAsync(cliente), Times.Once);
    }
}
