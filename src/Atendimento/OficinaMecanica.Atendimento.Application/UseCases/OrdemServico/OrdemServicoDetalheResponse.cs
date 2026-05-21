using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public record ItemServicoResponse(Guid Id, Guid ServicoId, int Quantidade, decimal ValorUnitario, decimal ValorTotal);
public record ItemPecaResponse(Guid Id, Guid PecaId, int Quantidade, decimal ValorUnitario, decimal ValorTotal);

public record OrdemServicoDetalheResponse(
    Guid Id,
    string Numero,
    StatusOrdemServico Status,
    Guid ClienteId,
    Guid VeiculoId,
    string Observacoes,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    decimal ValorTotal,
    IReadOnlyCollection<ItemServicoResponse> Servicos,
    IReadOnlyCollection<ItemPecaResponse> Pecas,
    IReadOnlyCollection<HistoricoStatusOSItem> Historico);

public record AcompanhamentoOSResponse(
    string Numero,
    StatusOrdemServico Status,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    IReadOnlyCollection<ItemServicoResponse> Servicos,
    IReadOnlyCollection<ItemPecaResponse> Pecas,
    IReadOnlyCollection<HistoricoStatusOSItem> Historico);

public record TempoMedioResponse(double TempoMedioHoras, int TotalOSFinalizadas);
public record TempoIndividualResponse(string Numero, StatusOrdemServico Status, double TempoDecorridoHoras, bool Finalizada);
