using FluentAssertions;
using TechChallenge.Domain.Validators;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class CpfValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    [InlineData("11144477735")]
    [InlineData("123.456.789-09")]
    [InlineData("12345678909")]
    public void EhValido_CpfValido_DeveRetornarTrue(string cpf)
        => CpfValidator.EhValido(cpf).Should().BeTrue();

    [Theory]
    [InlineData("000.000.000-00")]
    [InlineData("111.111.111-11")]
    [InlineData("529.982.247-26")]
    [InlineData("12345678")]
    [InlineData("")]
    [InlineData("   ")]
    public void EhValido_CpfInvalido_DeveRetornarFalse(string cpf)
        => CpfValidator.EhValido(cpf).Should().BeFalse();

    [Fact]
    public void Validar_CpfValido_NaoDeveLancarExcecao()
    {
        var act = () => CpfValidator.Validar("529.982.247-25");
        act.Should().NotThrow();
    }

    [Fact]
    public void Validar_CpfInvalido_DeveLancarArgumentException()
    {
        var act = () => CpfValidator.Validar("123.456.789-00");
        act.Should().Throw<ArgumentException>().WithMessage("*CPF inválido*");
    }

    [Fact]
    public void Validar_CpfNulo_DeveLancarArgumentException()
    {
        var act = () => CpfValidator.Validar(null!);
        act.Should().Throw<ArgumentException>();
    }
}
