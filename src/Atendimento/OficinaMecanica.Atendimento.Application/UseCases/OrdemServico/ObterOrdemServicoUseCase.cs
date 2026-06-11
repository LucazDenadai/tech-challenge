using OficinaMecanica.Atendimento.Application.Ports.Out;
using DomainOS = OficinaMecanica.Atendimento.Domain.Entities.OrdemServico;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class ObterOrdemServicoUseCase(IOrdemServicoRepository repository)
{
    public async Task<OrdemServicoDetalheResponse?> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var os = await repository.ObterComDetalhesAsync(id, ct);
        return os is null ? null : ToDetalhe(os);
    }

    internal static OrdemServicoDetalheResponse ToDetalhe(DomainOS os) => new(
        os.Id, os.Numero, os.Status, os.ClienteId, os.VeiculoId,
        os.Observacoes, os.DataAbertura, os.DataFechamento, os.ValorTotal,
        os.ItensServico.Select(i => new ItemServicoResponse(i.Id, i.ServicoId, i.Quantidade, i.ValorUnitario, i.ValorTotal)).ToList(),
        os.ItensPeca.Select(i => new ItemPecaResponse(i.Id, i.PecaId, i.Quantidade, i.ValorUnitario, i.ValorTotal)).ToList(),
        os.Historico.Select(h => new HistoricoStatusOSItem(h.StatusAnterior, h.StatusNovo, h.DataAlteracao)).ToList());
}
