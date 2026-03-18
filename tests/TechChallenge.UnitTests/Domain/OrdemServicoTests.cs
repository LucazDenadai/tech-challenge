using FluentAssertions;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class OrdemServicoTests
{
    private static OrdemServico CriarOS() =>
        new("OS-001", Guid.NewGuid(), Guid.NewGuid(), "Observação de teste");

    [Fact]
    public void AvancarStatus_DeRecebida_DeveIrParaEmDiagnostico()
    {
        var os = CriarOS();

        os.AvancarStatus();

        os.Status.Should().Be(StatusOrdemServico.EmDiagnostico);
    }

    [Theory]
    [InlineData(1, StatusOrdemServico.EmDiagnostico)]
    [InlineData(2, StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(3, StatusOrdemServico.EmExecucao)]
    [InlineData(4, StatusOrdemServico.Finalizada)]
    [InlineData(5, StatusOrdemServico.Entregue)]
    public void AvancarStatus_DevePercorrerSequenciaCorreta(int avancos, StatusOrdemServico statusEsperado)
    {
        var os = CriarOS();

        for (int i = 0; i < avancos; i++)
            os.AvancarStatus();

        os.Status.Should().Be(statusEsperado);
    }

    [Fact]
    public void AvancarStatus_QuandoEntregue_DeveLancarExcecao()
    {
        var os = CriarOS();
        for (int i = 0; i < 5; i++) os.AvancarStatus();

        var act = () => os.AvancarStatus();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*status final*");
    }

    [Fact]
    public void AvancarStatus_QuandoFinalizada_DeveDefinirDataFechamento()
    {
        var os = CriarOS();
        for (int i = 0; i < 4; i++) os.AvancarStatus(); // -> Finalizada

        os.DataFechamento.Should().NotBeNull();
        os.DataFechamento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AvancarStatus_QuandoEntregue_DeveManterDataFechamento()
    {
        var os = CriarOS();
        for (int i = 0; i < 5; i++) os.AvancarStatus(); // -> Entregue

        os.DataFechamento.Should().NotBeNull();
    }

    [Fact]
    public void ValorTotal_SemItens_DeveSerZero()
    {
        var os = CriarOS();

        os.ValorTotal.Should().Be(0m);
    }

    [Fact]
    public void ValorTotal_ComItensServico_DeveCalcularCorretamente()
    {
        var os = CriarOS();
        os.AdicionarItemServico(new ItemServico(os.Id, Guid.NewGuid(), 2, 100m)); // 200
        os.AdicionarItemServico(new ItemServico(os.Id, Guid.NewGuid(), 1, 50m));  // 50

        os.ValorTotal.Should().Be(250m);
    }

    [Fact]
    public void ValorTotal_ComItensPeca_DeveCalcularCorretamente()
    {
        var os = CriarOS();
        os.AdicionarItemPeca(new ItemPeca(os.Id, Guid.NewGuid(), 3, 35m)); // 105

        os.ValorTotal.Should().Be(105m);
    }

    [Fact]
    public void ValorTotal_ComServicosEPecas_DeveSomarTudo()
    {
        var os = CriarOS();
        os.AdicionarItemServico(new ItemServico(os.Id, Guid.NewGuid(), 1, 80m));  // 80
        os.AdicionarItemPeca(new ItemPeca(os.Id, Guid.NewGuid(), 2, 35m));        // 70

        os.ValorTotal.Should().Be(150m);
    }

    [Fact]
    public void AtualizarObservacoes_DeveAlterarObservacoes()
    {
        var os = CriarOS();

        os.AtualizarObservacoes("Nova observação");

        os.Observacoes.Should().Be("Nova observação");
        os.AtualizadoEm.Should().NotBeNull();
    }
}
