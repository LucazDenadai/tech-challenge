namespace OficinaMecanica.Atendimento.Application.Events;

public record OsFinalizadaEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OcorridoEm { get; init; } = DateTimeOffset.UtcNow;
    public Guid OrdemServicoId { get; init; }
    public string NumeroOS { get; init; } = string.Empty;
    public IReadOnlyList<ItemBaixaDto> Itens { get; init; } = [];
}

public record ItemBaixaDto(Guid PecaId, int Quantidade);
