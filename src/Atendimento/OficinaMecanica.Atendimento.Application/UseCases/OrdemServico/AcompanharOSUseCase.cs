using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class AcompanharOSUseCase(IOrdemServicoRepository repository)
{
    public async Task<AcompanhamentoOSResponse?> ExecutarAsync(string numero, CancellationToken ct = default)
    {
        var os = await repository.ObterPorNumeroAsync(numero, ct);
        if (os is null) return null;

        return new AcompanhamentoOSResponse(
            os.Numero, os.Status, os.DataAbertura, os.DataFechamento,
            os.ItensServico.Select(i => new ItemServicoResponse(i.Id, i.ServicoId, i.Quantidade, i.ValorUnitario, i.ValorTotal)).ToList(),
            os.ItensPeca.Select(i => new ItemPecaResponse(i.Id, i.PecaId, i.Quantidade, i.ValorUnitario, i.ValorTotal)).ToList(),
            os.Historico.Select(h => new HistoricoStatusOSItem(h.StatusAnterior, h.StatusNovo, h.DataAlteracao)).ToList());
    }
}
