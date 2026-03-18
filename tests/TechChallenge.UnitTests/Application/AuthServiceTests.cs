using FluentAssertions;
using Moq;
using TechChallenge.Application.DTOs.Auth;
using TechChallenge.Application.Services;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Interfaces;
using Xunit;

namespace TechChallenge.UnitTests.Application;

public class AuthServiceTests : IDisposable
{
    private readonly Mock<IUsuarioRepository> _repoMock = new();
    private readonly AuthService _sut;

    private const string JwtKey = "AuthServiceTests_SecretKey_MinLength32Chars!";
    private const string JwtIssuer = "TechChallenge.API";
    private const string JwtAudience = "TechChallenge.Client";

    public AuthServiceTests()
    {
        Environment.SetEnvironmentVariable("JWT_KEY", JwtKey);
        Environment.SetEnvironmentVariable("JWT_ISSUER", JwtIssuer);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", JwtAudience);
        Environment.SetEnvironmentVariable("JWT_EXPIRACAO_MINUTOS", "60");

        _sut = new AuthService(_repoMock.Object);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("JWT_KEY", null);
        Environment.SetEnvironmentVariable("JWT_ISSUER", null);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);
        Environment.SetEnvironmentVariable("JWT_EXPIRACAO_MINUTOS", null);
    }

    [Fact]
    public async Task LoginAsync_CredenciaisInvalidas_UsuarioNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.ObterPorEmailAsync("naoexiste@email.com")).ReturnsAsync((Usuario?)null);

        var resultado = await _sut.LoginAsync(new LoginDto { Email = "naoexiste@email.com", Senha = "qualquer" });

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UsuarioInativo_DeveRetornarNull()
    {
        var usuario = CriarUsuarioInativo();
        _repoMock.Setup(r => r.ObterPorEmailAsync(usuario.Email)).ReturnsAsync(usuario);

        var resultado = await _sut.LoginAsync(new LoginDto { Email = usuario.Email, Senha = "Admin@123" });

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_SenhaErrada_DeveRetornarNull()
    {
        var usuario = CriarUsuario();
        _repoMock.Setup(r => r.ObterPorEmailAsync(usuario.Email)).ReturnsAsync(usuario);

        var resultado = await _sut.LoginAsync(new LoginDto { Email = usuario.Email, Senha = "SenhaErrada" });

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_CredenciaisValidas_DeveRetornarToken()
    {
        var usuario = CriarUsuario();
        _repoMock.Setup(r => r.ObterPorEmailAsync(usuario.Email)).ReturnsAsync(usuario);

        var resultado = await _sut.LoginAsync(new LoginDto { Email = usuario.Email, Senha = "Admin@123" });

        resultado.Should().NotBeNull();
        resultado!.Token.Should().NotBeNullOrEmpty();
        resultado.Email.Should().Be(usuario.Email);
        resultado.Perfil.Should().Be(PerfilUsuario.Admin.ToString());
    }

    private static Usuario CriarUsuario() =>
        new("Admin Teste", "admin@teste.com", BCrypt.Net.BCrypt.HashPassword("Admin@123"), PerfilUsuario.Admin);

    private static Usuario CriarUsuarioInativo()
    {
        var u = new Usuario("Inativo", "inativo@teste.com", BCrypt.Net.BCrypt.HashPassword("Admin@123"), PerfilUsuario.Admin);
        u.Desativar();
        return u;
    }
}
