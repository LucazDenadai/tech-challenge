using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public record ConsultarStatusOSResponse(
    Guid Id,
    string Numero,
    StatusOrdemServico Status,
    decimal ValorTotal,
    IReadOnlyCollection<HistoricoStatusOSItem> Historico
);

public record HistoricoStatusOSItem(
    StatusOrdemServico StatusAnterior,
    StatusOrdemServico StatusNovo,
    DateTime AlteradoEm
);
