namespace OficinaMecanica.Estoque.Application.Events;

// Contrato espelhado do Atendimento — MassTransit identifica pelo nome do tipo
public record OsFinalizadaEvent
{
    public Guid EventId { get; init; }
    public DateTimeOffset OcorridoEm { get; init; }
    public Guid OrdemServicoId { get; init; }
    public string NumeroOS { get; init; } = string.Empty;
    public IReadOnlyList<ItemBaixaDto> Itens { get; init; } = [];
}

public record ItemBaixaDto(Guid PecaId, int Quantidade);
