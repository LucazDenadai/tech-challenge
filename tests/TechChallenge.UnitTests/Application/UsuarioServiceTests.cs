using FluentAssertions;
using Moq;
using TechChallenge.Application.DTOs.Usuario;
using TechChallenge.Application.Services;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Interfaces;
using Xunit;

namespace TechChallenge.UnitTests.Application;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repoMock = new();
    private readonly UsuarioService _sut;

    public UsuarioServiceTests()
    {
        _sut = new UsuarioService(_repoMock.Object);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaMapeada()
    {
        var usuarios = new[]
        {
            new Usuario("Admin", "admin@email.com", "hash", PerfilUsuario.Admin),
            new Usuario("Mecanico", "mec@email.com", "hash", PerfilUsuario.Mecanico),
        };
        _repoMock.Setup(r => r.ObterTodosAsync()).ReturnsAsync(usuarios);

        var resultado = await _sut.ObterTodosAsync();

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterPorIdAsync_UsuarioExiste_DeveRetornarDto()
    {
        var id = Guid.NewGuid();
        var usuario = new Usuario("Admin", "admin@email.com", "hash", PerfilUsuario.Admin);
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);

        var resultado = await _sut.ObterPorIdAsync(id);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Admin");
        resultado.Email.Should().Be("admin@email.com");
        resultado.Perfil.Should().Be(PerfilUsuario.Admin);
    }

    [Fact]
    public async Task ObterPorIdAsync_UsuarioNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Usuario?)null);

        var resultado = await _sut.ObterPorIdAsync(Guid.NewGuid());

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CriarAsync_EmailJaCadastrado_DeveLancarExcecao()
    {
        var dto = new CriarUsuarioDto { Nome = "Admin", Email = "admin@email.com", Senha = "Admin@123", Perfil = PerfilUsuario.Admin };
        _repoMock.Setup(r => r.EmailExisteAsync(dto.Email, null)).ReturnsAsync(true);

        var act = async () => await _sut.CriarAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*E-mail já cadastrado*");
    }

    [Fact]
    public async Task CriarAsync_DadosValidos_DeveCriarERetornarDto()
    {
        var dto = new CriarUsuarioDto { Nome = "Novo Usuario", Email = "novo@email.com", Senha = "Senha@123", Perfil = PerfilUsuario.Mecanico };
        _repoMock.Setup(r => r.EmailExisteAsync(dto.Email, null)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.CriarAsync(dto);

        resultado.Nome.Should().Be("Novo Usuario");
        resultado.Email.Should().Be("novo@email.com");
        resultado.Perfil.Should().Be(PerfilUsuario.Mecanico);
        resultado.Ativo.Should().BeTrue();
        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
        _repoMock.Verify(r => r.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_UsuarioNaoEncontrado_DeveLancarExcecao()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Usuario?)null);

        var act = async () => await _sut.AtualizarAsync(id, new CriarUsuarioDto());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Usuário não encontrado*");
    }

    [Fact]
    public async Task AtualizarAsync_EmailDuplicado_DeveLancarExcecao()
    {
        var id = Guid.NewGuid();
        var usuario = new Usuario("Admin", "admin@email.com", "hash", PerfilUsuario.Admin);
        var dto = new CriarUsuarioDto { Nome = "Admin", Email = "outro@email.com", Senha = "", Perfil = PerfilUsuario.Admin };
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.EmailExisteAsync(dto.Email, id)).ReturnsAsync(true);

        var act = async () => await _sut.AtualizarAsync(id, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*E-mail já cadastrado*");
    }

    [Fact]
    public async Task AtualizarAsync_SemNovaSenha_NaoAlteraSenhaHash()
    {
        var id = Guid.NewGuid();
        var senhaHashOriginal = BCrypt.Net.BCrypt.HashPassword("Senha@123");
        var usuario = new Usuario("Admin", "admin@email.com", senhaHashOriginal, PerfilUsuario.Admin);
        var dto = new CriarUsuarioDto { Nome = "Admin Atualizado", Email = "admin@email.com", Senha = "", Perfil = PerfilUsuario.Admin };
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.EmailExisteAsync(dto.Email, id)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var resultado = await _sut.AtualizarAsync(id, dto);

        resultado.Nome.Should().Be("Admin Atualizado");
        usuario.SenhaHash.Should().Be(senhaHashOriginal);
    }

    [Fact]
    public async Task AtualizarAsync_ComNovaSenha_DeveAlterarSenhaHash()
    {
        var id = Guid.NewGuid();
        var senhaHashOriginal = BCrypt.Net.BCrypt.HashPassword("Senha@123");
        var usuario = new Usuario("Admin", "admin@email.com", senhaHashOriginal, PerfilUsuario.Admin);
        var dto = new CriarUsuarioDto { Nome = "Admin", Email = "admin@email.com", Senha = "NovaSenha@456", Perfil = PerfilUsuario.Admin };
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.EmailExisteAsync(dto.Email, id)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        await _sut.AtualizarAsync(id, dto);

        usuario.SenhaHash.Should().NotBe(senhaHashOriginal);
    }

    [Fact]
    public async Task DesativarAsync_UsuarioNaoEncontrado_DeveLancarExcecao()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Usuario?)null);

        var act = async () => await _sut.DesativarAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DesativarAsync_UsuarioExiste_DeveDesativar()
    {
        var id = Guid.NewGuid();
        var usuario = new Usuario("Admin", "admin@email.com", "hash", PerfilUsuario.Admin);
        _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        await _sut.DesativarAsync(id);

        usuario.Ativo.Should().BeFalse();
        _repoMock.Verify(r => r.AtualizarAsync(usuario), Times.Once);
    }
}
