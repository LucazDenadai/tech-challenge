using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public record AbrirOrdemServicoRequest(
    Guid ClienteId,
    Guid VeiculoId,
    string Observacoes,
    IEnumerable<ItemPecaRequest> Pecas
);
