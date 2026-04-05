using FluentAssertions;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using Xunit;

namespace TechChallenge.UnitTests.Domain;

public class OrdemServicoTests
{
    private static OrdemServico CriarOrdemServico() =>
        new("OS-2026-0001", Guid.NewGuid(), Guid.NewGuid(), "Observação inicial");

    [Fact]
    public void Construtor_DevePreencharPropriedadesCorretamente()
    {
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();

        var os = new OrdemServico("OS-2026-0001", clienteId, veiculoId, "Teste");

        os.Numero.Should().Be("OS-2026-0001");
        os.ClienteId.Should().Be(clienteId);
        os.VeiculoId.Should().Be(veiculoId);
        os.Observacoes.Should().Be("Teste");
        os.Status.Should().Be(StatusOrdemServico.Recebida);
        os.DataFechamento.Should().BeNull();
    }

    [Fact]
    public void AvancarStatus_DeRecebidaParaEmDiagnostico_DeveAtualizar()
    {
        var os = CriarOrdemServico();

        os.AvancarStatus();

        os.Status.Should().Be(StatusOrdemServico.EmDiagnostico);
        os.DataFechamento.Should().BeNull();
    }

    [Fact]
    public void AvancarStatus_TodasTransicoesAteEntregue_DeveSetarDataFechamento()
    {
        var os = CriarOrdemServico();

        os.AvancarStatus(); // Recebida → EmDiagnostico
        os.AvancarStatus(); // EmDiagnostico → AguardandoAprovacao
        os.AvancarStatus(); // AguardandoAprovacao → EmExecucao
        os.AvancarStatus(); // EmExecucao → Finalizada
        os.DataFechamento.Should().NotBeNull();

        os.AvancarStatus(); // Finalizada → Entregue
        os.Status.Should().Be(StatusOrdemServico.Entregue);
        os.DataFechamento.Should().NotBeNull();
    }

    [Fact]
    public void AvancarStatus_StatusFinal_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();

        os.AvancarStatus(); // → EmDiagnostico
        os.AvancarStatus(); // → AguardandoAprovacao
        os.AvancarStatus(); // → EmExecucao
        os.AvancarStatus(); // → Finalizada
        os.AvancarStatus(); // → Entregue

        os.Invoking(o => o.AvancarStatus())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("OS já está no status final.");
    }

    [Fact]
    public void AtualizarObservacoes_DeveAlterarObservacoes()
    {
        var os = CriarOrdemServico();

        os.AtualizarObservacoes("Nova observação");

        os.Observacoes.Should().Be("Nova observação");
    }

    [Fact]
    public void AdicionarItemServico_DeveAdicionar()
    {
        var os = CriarOrdemServico();
        var item = new ItemServico(os.Id, Guid.NewGuid(), 2, 100m);

        os.AdicionarItemServico(item);

        os.ItensServico.Should().ContainSingle();
        os.ValorTotal.Should().Be(200m);
    }

    [Fact]
    public void AdicionarItemPeca_DeveAdicionar()
    {
        var os = CriarOrdemServico();
        var item = new ItemPeca(os.Id, Guid.NewGuid(), 3, 50m);

        os.AdicionarItemPeca(item);

        os.ItensPeca.Should().ContainSingle();
        os.ValorTotal.Should().Be(150m);
    }
}
