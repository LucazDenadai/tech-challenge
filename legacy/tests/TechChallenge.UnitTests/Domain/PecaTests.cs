using FluentAssertions;
using TechChallenge.Domain.Entities;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class PecaTests
{
    private static Peca CriarPeca(int estoque = 10) =>
        new("Filtro de Óleo", "Filtro universal", 35m, estoque);

    [Fact]
    public void ConsumirEstoque_QuantidadeSuficiente_DeveReduzirEstoque()
    {
        var peca = CriarPeca(10);

        peca.ConsumirEstoque(3);

        peca.QuantidadeEstoque.Should().Be(7);
    }

    [Fact]
    public void ConsumirEstoque_EstoqueExato_DeveZerarEstoque()
    {
        var peca = CriarPeca(5);

        peca.ConsumirEstoque(5);

        peca.QuantidadeEstoque.Should().Be(0);
    }

    [Fact]
    public void ConsumirEstoque_QuantidadeInsuficiente_DeveLancarExcecao()
    {
        var peca = CriarPeca(2);

        var act = () => peca.ConsumirEstoque(5);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Estoque insuficiente*");
    }

    [Fact]
    public void ConsumirEstoque_QuantidadeZero_DeveLancarExcecao()
    {
        var peca = CriarPeca(10);

        var act = () => peca.ConsumirEstoque(0);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*positiva*");
    }

    [Fact]
    public void ConsumirEstoque_QuantidadeNegativa_DeveLancarExcecao()
    {
        var peca = CriarPeca(10);

        var act = () => peca.ConsumirEstoque(-1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AdicionarEstoque_QuantidadePositiva_DeveAumentarEstoque()
    {
        var peca = CriarPeca(10);

        peca.AdicionarEstoque(5);

        peca.QuantidadeEstoque.Should().Be(15);
    }

    [Fact]
    public void AdicionarEstoque_QuantidadeZero_DeveLancarExcecao()
    {
        var peca = CriarPeca(10);

        var act = () => peca.AdicionarEstoque(0);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AdicionarEstoque_QuantidadeNegativa_DeveLancarExcecao()
    {
        var peca = CriarPeca(10);

        var act = () => peca.AdicionarEstoque(-3);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Desativar_DeveMudarAtivoParaFalse()
    {
        var peca = CriarPeca();

        peca.Desativar();

        peca.Ativo.Should().BeFalse();
        peca.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public void Atualizar_DeveAlterarDados()
    {
        var peca = CriarPeca();

        peca.Atualizar("Novo Nome", "Nova Desc", 99m);

        peca.Nome.Should().Be("Novo Nome");
        peca.Descricao.Should().Be("Nova Desc");
        peca.Preco.Should().Be(99m);
        peca.AtualizadoEm.Should().NotBeNull();
    }
}
