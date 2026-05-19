using FluentAssertions;
using TechChallenge.Domain.Validators;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class PlacaValidatorTests
{
    [Theory]
    [InlineData("ABC1234")]   // formato antigo
    [InlineData("ABC-1234")]  // antigo com hífen
    [InlineData("ABC1D23")]   // Mercosul
    [InlineData("abc1234")]   // minúsculo — deve normalizar
    [InlineData("abc-1234")]  // minúsculo com hífen
    public void EhValido_PlacaValida_DeveRetornarTrue(string placa)
        => PlacaValidator.EhValido(placa).Should().BeTrue();

    [Theory]
    [InlineData("ABC123")]    // curta demais
    [InlineData("ABCD1234")]  // longa demais
    [InlineData("1BC1234")]   // começa com número
    [InlineData("ABC12D4")]   // Mercosul inválido (letra na posição errada)
    [InlineData("")]
    [InlineData("   ")]
    public void EhValido_PlacaInvalida_DeveRetornarFalse(string placa)
        => PlacaValidator.EhValido(placa).Should().BeFalse();

    [Fact]
    public void Validar_PlacaValida_NaoDeveLancarExcecao()
    {
        var act = () => PlacaValidator.Validar("ABC-1234");
        act.Should().NotThrow();
    }

    [Fact]
    public void Validar_PlacaInvalida_DeveLancarArgumentException()
    {
        var act = () => PlacaValidator.Validar("INVALIDA");
        act.Should().Throw<ArgumentException>().WithMessage("*Placa*");
    }

    [Theory]
    [InlineData("abc-1234", "ABC1234")]
    [InlineData("ABC 1234", "ABC1234")]
    [InlineData("ABC1D23", "ABC1D23")]
    public void Normalizar_DeveRetornarPlacaSemHifenMaiuscula(string entrada, string esperado)
        => PlacaValidator.Normalizar(entrada).Should().Be(esperado);
}
