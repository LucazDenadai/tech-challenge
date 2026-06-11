using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public record ListarOrdensServicoItem(
    Guid Id,
    string Numero,
    StatusOrdemServico Status,
    DateTime DataAbertura,
    decimal ValorTotal
);
