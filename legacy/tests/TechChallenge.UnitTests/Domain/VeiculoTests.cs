using FluentAssertions;
using TechChallenge.Domain.Entities;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class VeiculoTests
{
    private static Veiculo CriarVeiculo() =>
        new(Guid.NewGuid(), "ABC1234", "Toyota", "Corolla", 2020, "Prata");

    [Fact]
    public void Construtor_DevePreencharPropriedadesCorretamente()
    {
        var clienteId = Guid.NewGuid();
        var veiculo = new Veiculo(clienteId, "ABC1234", "Toyota", "Corolla", 2020, "Prata");

        veiculo.ClienteId.Should().Be(clienteId);
        veiculo.Placa.Should().Be("ABC1234");
        veiculo.Marca.Should().Be("Toyota");
        veiculo.Modelo.Should().Be("Corolla");
        veiculo.Ano.Should().Be(2020);
        veiculo.Cor.Should().Be("Prata");
        veiculo.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Atualizar_DeveAlterarDados()
    {
        var veiculo = CriarVeiculo();

        veiculo.Atualizar("Honda", "Civic", 2022, "Preto");

        veiculo.Marca.Should().Be("Honda");
        veiculo.Modelo.Should().Be("Civic");
        veiculo.Ano.Should().Be(2022);
        veiculo.Cor.Should().Be("Preto");
        veiculo.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public void Atualizar_NaoDeveMudarPlacaNemClienteId()
    {
        var clienteId = Guid.NewGuid();
        var veiculo = new Veiculo(clienteId, "ABC1234", "Toyota", "Corolla", 2020, "Prata");

        veiculo.Atualizar("Honda", "Civic", 2022, "Preto");

        veiculo.Placa.Should().Be("ABC1234");
        veiculo.ClienteId.Should().Be(clienteId);
    }
}
