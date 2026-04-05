using FluentAssertions;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class UsuarioTests
{
    private static Usuario CriarUsuario() =>
        new("Admin Teste", "admin@teste.com", "hash_original", PerfilUsuario.Admin);

    [Fact]
    public void Construtor_DevePreencharPropriedadesCorretamente()
    {
        var usuario = CriarUsuario();

        usuario.Nome.Should().Be("Admin Teste");
        usuario.Email.Should().Be("admin@teste.com");
        usuario.SenhaHash.Should().Be("hash_original");
        usuario.Perfil.Should().Be(PerfilUsuario.Admin);
        usuario.Ativo.Should().BeTrue();
        usuario.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Atualizar_DeveAlterarDados()
    {
        var usuario = CriarUsuario();

        usuario.Atualizar("Novo Nome", "novo@email.com", PerfilUsuario.Mecanico);

        usuario.Nome.Should().Be("Novo Nome");
        usuario.Email.Should().Be("novo@email.com");
        usuario.Perfil.Should().Be(PerfilUsuario.Mecanico);
        usuario.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public void AlterarSenha_DeveAtualizarSenhaHash()
    {
        var usuario = CriarUsuario();

        usuario.AlterarSenha("novo_hash");

        usuario.SenhaHash.Should().Be("novo_hash");
        usuario.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public void Desativar_DeveMudarAtivoParaFalse()
    {
        var usuario = CriarUsuario();

        usuario.Desativar();

        usuario.Ativo.Should().BeFalse();
        usuario.AtualizadoEm.Should().NotBeNull();
    }
}
