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
    public void AlterarStatus_DeRecebidaParaEmDiagnostico_DeveAtualizar()
    {
        var os = CriarOrdemServico();

        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);

        os.Status.Should().Be(StatusOrdemServico.EmDiagnostico);
        os.DataFechamento.Should().BeNull();
    }

    [Fact]
    public void AlterarStatus_TodasTransicoesAteEntregue_DeveSetarDataFechamento()
    {
        var os = CriarOrdemServico();

        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);
        os.AlterarStatus(StatusOrdemServico.EmExecucao);
        os.AlterarStatus(StatusOrdemServico.Finalizada);
        os.DataFechamento.Should().NotBeNull();

        os.AlterarStatus(StatusOrdemServico.Entregue);
        os.Status.Should().Be(StatusOrdemServico.Entregue);
        os.DataFechamento.Should().NotBeNull();
    }

    [Fact]
    public void AlterarStatus_StatusFinal_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);
        os.AlterarStatus(StatusOrdemServico.EmExecucao);
        os.AlterarStatus(StatusOrdemServico.Finalizada);
        os.AlterarStatus(StatusOrdemServico.Entregue);

        os.Invoking(o => o.AlterarStatus(StatusOrdemServico.EmDiagnostico))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*não pode ser alterada*");
    }

    [Fact]
    public void AlterarStatus_Cancelada_DeveSetarDataFechamentoEImpedirNovasAlteracoes()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);

        os.AlterarStatus(StatusOrdemServico.Cancelada);

        os.Status.Should().Be(StatusOrdemServico.Cancelada);
        os.DataFechamento.Should().NotBeNull();

        os.Invoking(o => o.AlterarStatus(StatusOrdemServico.EmDiagnostico))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AlterarStatus_StatusNaoSequencial_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();

        os.Invoking(o => o.AlterarStatus(StatusOrdemServico.Entregue))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*próximo status esperado*");
    }

    [Fact]
    public void AlterarStatus_DeveRegistrarHistorico()
    {
        var os = CriarOrdemServico();

        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);

        os.Historico.Should().ContainSingle();
        os.Historico.First().StatusAnterior.Should().Be(StatusOrdemServico.Recebida);
        os.Historico.First().StatusNovo.Should().Be(StatusOrdemServico.EmDiagnostico);
    }

    [Fact]
    public void AtualizarObservacoes_DeveAlterarObservacoes()
    {
        var os = CriarOrdemServico();

        os.AtualizarObservacoes("Nova observação");

        os.Observacoes.Should().Be("Nova observação");
    }

    [Fact]
    public void AdicionarItemServico_EmDiagnostico_DeveAdicionar()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var item = new ItemServico(os.Id, Guid.NewGuid(), 2, 100m);

        os.AdicionarItemServico(item);

        os.ItensServico.Should().ContainSingle();
        os.ValorTotal.Should().Be(200m);
    }

    [Fact]
    public void AdicionarItemServico_StatusInvalido_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();
        var item = new ItemServico(os.Id, Guid.NewGuid(), 1, 100m);

        os.Invoking(o => o.AdicionarItemServico(item))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AdicionarItemPeca_EmDiagnostico_DeveAdicionar()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var item = new ItemPeca(os.Id, Guid.NewGuid(), 3, 50m);

        os.AdicionarItemPeca(item);

        os.ItensPeca.Should().ContainSingle();
        os.ValorTotal.Should().Be(150m);
    }

    [Fact]
    public void AdicionarItemPeca_StatusInvalido_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();
        var item = new ItemPeca(os.Id, Guid.NewGuid(), 1, 50m);

        os.Invoking(o => o.AdicionarItemPeca(item))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AdicionarItemServico_ServicoJaAdicionado_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var servicoId = Guid.NewGuid();
        os.AdicionarItemServico(new ItemServico(os.Id, servicoId, 1, 100m));

        os.Invoking(o => o.AdicionarItemServico(new ItemServico(os.Id, servicoId, 1, 100m)))
            .Should().Throw<InvalidOperationException>().WithMessage("*já foi adicionado*");
    }

    [Fact]
    public void RemoverItemServico_EmDiagnostico_DeveRemoverItem()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var item = new ItemServico(os.Id, Guid.NewGuid(), 1, 100m);
        os.AdicionarItemServico(item);

        os.RemoverItemServico(item.Id);

        os.ItensServico.Should().BeEmpty();
    }

    [Fact]
    public void RemoverItemServico_StatusInvalido_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);

        os.Invoking(o => o.RemoverItemServico(Guid.NewGuid()))
            .Should().Throw<InvalidOperationException>().WithMessage("*diagnóstico*");
    }

    [Fact]
    public void RemoverItemServico_ItemNaoEncontrado_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);

        os.Invoking(o => o.RemoverItemServico(Guid.NewGuid()))
            .Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void RemoverItemPeca_EmDiagnostico_DeveRemoverItem()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        var item = new ItemPeca(os.Id, Guid.NewGuid(), 2, 50m);
        os.AdicionarItemPeca(item);

        os.RemoverItemPeca(item.Id);

        os.ItensPeca.Should().BeEmpty();
    }

    [Fact]
    public void RemoverItemPeca_StatusInvalido_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);
        os.AlterarStatus(StatusOrdemServico.AguardandoAprovacao);

        os.Invoking(o => o.RemoverItemPeca(Guid.NewGuid()))
            .Should().Throw<InvalidOperationException>().WithMessage("*diagnóstico*");
    }

    [Fact]
    public void RemoverItemPeca_ItemNaoEncontrado_DeveLancarExcecao()
    {
        var os = CriarOrdemServico();
        os.AlterarStatus(StatusOrdemServico.EmDiagnostico);

        os.Invoking(o => o.RemoverItemPeca(Guid.NewGuid()))
            .Should().Throw<KeyNotFoundException>();
    }
}
