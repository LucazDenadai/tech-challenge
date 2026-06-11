using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class ListarOrdensServicoUseCase(IOrdemServicoRepository repository)
{
    private static readonly Dictionary<StatusOrdemServico, int> _prioridade = new()
    {
        [StatusOrdemServico.EmExecucao] = 1,
        [StatusOrdemServico.AguardandoAprovacao] = 2,
        [StatusOrdemServico.EmDiagnostico] = 3,
        [StatusOrdemServico.Recebida] = 4,
    };

    public async Task<IReadOnlyCollection<ListarOrdensServicoItem>> ExecutarAsync(CancellationToken ct = default)
    {
        var todas = await repository.ObterTodosAsync(ct);

        return todas
            .Where(os => _prioridade.ContainsKey(os.Status))
            .OrderBy(os => _prioridade[os.Status])
            .ThenBy(os => os.DataAbertura)
            .Select(os => new ListarOrdensServicoItem(os.Id, os.Numero, os.Status, os.DataAbertura, os.ValorTotal))
            .ToList()
            .AsReadOnly();
    }
}
