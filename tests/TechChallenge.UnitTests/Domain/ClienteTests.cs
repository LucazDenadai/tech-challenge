using FluentAssertions;
using TechChallenge.Domain.Entities;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class ClienteTests
{
    private static Cliente CriarCliente() =>
        new("Carlos Silva", "52998224725", "carlos@email.com", "11987654321", "Rua das Flores, 100");

    [Fact]
    public void Construtor_DevePreencharPropriedadesCorretamente()
    {
        var cliente = CriarCliente();

        cliente.Nome.Should().Be("Carlos Silva");
        cliente.Documento.Should().Be("52998224725");
        cliente.Email.Should().Be("carlos@email.com");
        cliente.Ativo.Should().BeTrue();
        cliente.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Construtor_ComCnpjValido_DeveCriarCliente()
    {
        var cliente = new Cliente("Empresa LTDA", "11222333000181", "empresa@email.com", "1133334444", "Av. Comercial, 500");

        cliente.Documento.Should().Be("11222333000181");
    }

    [Fact]
    public void Construtor_ComDocumentoInvalido_DeveLancarExcecao()
    {
        var act = () => new Cliente("Nome", "00000000000", "e@e.com", "11999", "Rua");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Atualizar_DeveAlterarDados()
    {
        var cliente = CriarCliente();

        cliente.Atualizar("Novo Nome", "novo@email.com", "11900000000", "Nova Rua, 1");

        cliente.Nome.Should().Be("Novo Nome");
        cliente.Email.Should().Be("novo@email.com");
        cliente.Telefone.Should().Be("11900000000");
        cliente.Endereco.Should().Be("Nova Rua, 1");
        cliente.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public void Atualizar_NaoDeveMudarDocumento()
    {
        var cliente = CriarCliente();

        cliente.Atualizar("Outro Nome", "outro@email.com", "11911111111", "Outra Rua");

        cliente.Documento.Should().Be("52998224725");
    }

    [Fact]
    public void Desativar_DeveMudarAtivoParaFalse()
    {
        var cliente = CriarCliente();

        cliente.Desativar();

        cliente.Ativo.Should().BeFalse();
        cliente.AtualizadoEm.Should().NotBeNull();
    }
}
