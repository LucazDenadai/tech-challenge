using FluentAssertions;
using TechChallenge.Domain.Entities;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class ServicoTests
{
    private static Servico CriarServico() =>
        new("Troca de Óleo", "Troca completa do óleo do motor", 80m, 30);

    [Fact]
    public void Construtor_DevePreencharPropriedadesCorretamente()
    {
        var servico = CriarServico();

        servico.Nome.Should().Be("Troca de Óleo");
        servico.Descricao.Should().Be("Troca completa do óleo do motor");
        servico.Preco.Should().Be(80m);
        servico.TempoConclusaoMinutos.Should().Be(30);
        servico.Ativo.Should().BeTrue();
        servico.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Atualizar_DeveAlterarDados()
    {
        var servico = CriarServico();

        servico.Atualizar("Alinhamento", "Alinhamento das rodas", 120m, 60);

        servico.Nome.Should().Be("Alinhamento");
        servico.Descricao.Should().Be("Alinhamento das rodas");
        servico.Preco.Should().Be(120m);
        servico.TempoConclusaoMinutos.Should().Be(60);
        servico.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public void Desativar_DeveMudarAtivoParaFalse()
    {
        var servico = CriarServico();

        servico.Desativar();

        servico.Ativo.Should().BeFalse();
        servico.AtualizadoEm.Should().NotBeNull();
    }
}
