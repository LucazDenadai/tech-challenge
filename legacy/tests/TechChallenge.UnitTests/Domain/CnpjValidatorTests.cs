using FluentAssertions;
using TechChallenge.Domain.Validators;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class CnpjValidatorTests
{
    [Theory]
    [InlineData("11.222.333/0001-81")]
    [InlineData("11222333000181")]
    [InlineData("45.997.418/0001-53")]
    [InlineData("45997418000153")]
    [InlineData("12.345.678/0001-95")]
    [InlineData("12345678000195")]
    public void EhValido_CnpjValido_DeveRetornarTrue(string cnpj)
        => CnpjValidator.Valido(cnpj).Should().BeTrue();

    [Theory]
    [InlineData("00.000.000/0000-00")]
    [InlineData("11.111.111/1111-11")]
    [InlineData("11.222.333/0001-82")]
    [InlineData("1234567")]
    [InlineData("")]
    [InlineData("   ")]
    public void EhValido_CnpjInvalido_DeveRetornarFalse(string cnpj)
        => CnpjValidator.Valido(cnpj).Should().BeFalse();

    [Fact]
    public void Validar_CnpjValido_NaoDeveLancarExcecao()
    {
        var act = () => CnpjValidator.Validar("11.222.333/0001-81");
        act.Should().NotThrow();
    }

    [Fact]
    public void Validar_CnpjInvalido_DeveLancarArgumentException()
    {
        var act = () => CnpjValidator.Validar("11.222.333/0001-00");
        act.Should().Throw<ArgumentException>().WithMessage("*CNPJ inválido*");
    }

    [Fact]
    public void Validar_CnpjNulo_DeveLancarArgumentException()
    {
        var act = () => CnpjValidator.Validar(null!);
        act.Should().Throw<ArgumentException>();
    }
}
