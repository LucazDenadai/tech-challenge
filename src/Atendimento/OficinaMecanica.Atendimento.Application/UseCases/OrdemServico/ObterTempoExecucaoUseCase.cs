using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class ObterTempoExecucaoUseCase(IOrdemServicoRepository repository)
{
    public async Task<TempoMedioResponse> ObterTempoMedioAsync(CancellationToken ct = default)
    {
        var todas = await repository.ObterTodosAsync(ct);
        var finalizadas = todas
            .Where(os => os.DataFechamento.HasValue &&
                         os.Status is StatusOrdemServico.Finalizada or StatusOrdemServico.Entregue)
            .ToList();

        if (finalizadas.Count == 0)
            return new TempoMedioResponse(0, 0);

        var media = finalizadas.Average(os => (os.DataFechamento!.Value - os.DataAbertura).TotalHours);
        return new TempoMedioResponse(Math.Round(media, 2), finalizadas.Count);
    }

    public async Task<TempoIndividualResponse?> ObterTempoIndividualAsync(string numero, CancellationToken ct = default)
    {
        var os = await repository.ObterPorNumeroAsync(numero, ct);
        if (os is null) return null;

        var finalizada = os.DataFechamento.HasValue;
        var referencia = finalizada ? os.DataFechamento!.Value : DateTime.UtcNow;
        var horas = Math.Round((referencia - os.DataAbertura).TotalHours, 2);

        return new TempoIndividualResponse(os.Numero, os.Status, horas, finalizada);
    }
}
