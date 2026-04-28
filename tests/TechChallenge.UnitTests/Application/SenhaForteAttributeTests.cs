using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using TechChallenge.Application.Validators;
using Xunit;

namespace TechChallenge.UnitTests.Application;

public class SenhaForteAttributeTests
{
    private readonly SenhaForteAttribute _sut = new();

    private ValidationResult? Validar(string? senha)
    {
        var ctx = new ValidationContext(new object());
        return _sut.GetValidationResult(senha, ctx);
    }

    [Theory]
    [InlineData("Abc@1234")]
    [InlineData("Senha@123")]
    [InlineData("MinH@8901")]
    [InlineData("P@ssw0rd!")]
    public void SenhasFortes_DevemPassar(string senha)
        => Validar(senha).Should().Be(ValidationResult.Success);

    [Fact]
    public void SenhaNula_DeveRetornarErro()
        => Validar(null)!.ErrorMessage.Should().Contain("obrigatória");

    [Fact]
    public void SenhaVazia_DeveRetornarErro()
        => Validar(string.Empty)!.ErrorMessage.Should().Contain("obrigatória");

    [Fact]
    public void SenhaCurta_DeveRetornarErro()
        => Validar("Ab@1234")!.ErrorMessage.Should().Contain("mínimo de 8 caracteres");

    [Fact]
    public void SenhaSemMaiuscula_DeveRetornarErro()
        => Validar("abc@1234")!.ErrorMessage.Should().Contain("letra maiúscula");

    [Fact]
    public void SenhaSemMinuscula_DeveRetornarErro()
        => Validar("ABC@1234")!.ErrorMessage.Should().Contain("letra minúscula");

    [Fact]
    public void SenhaSemDigito_DeveRetornarErro()
        => Validar("Abcd@efg")!.ErrorMessage.Should().Contain("número");

    [Fact]
    public void SenhaSemEspecial_DeveRetornarErro()
        => Validar("Abcd1234")!.ErrorMessage.Should().Contain("caractere especial");

    [Fact]
    public void SenhaFraca_ListaTodosOsErros()
    {
        var resultado = Validar("abc");
        resultado!.ErrorMessage.Should().Contain("mínimo de 8 caracteres");
        resultado.ErrorMessage.Should().Contain("letra maiúscula");
        resultado.ErrorMessage.Should().Contain("número");
        resultado.ErrorMessage.Should().Contain("caractere especial");
    }
}
