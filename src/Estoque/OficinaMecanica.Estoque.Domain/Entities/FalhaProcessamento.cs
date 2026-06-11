namespace OficinaMecanica.Estoque.Domain.Entities;

public class FalhaProcessamento : EntityBase
{
    public Guid EventId { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public string Erro { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public DateTimeOffset OcorridoEm { get; private set; }

    protected FalhaProcessamento() { }

    public FalhaProcessamento(Guid eventId, Guid ordemServicoId, string erro, string payloadJson)
    {
        EventId = eventId;
        OrdemServicoId = ordemServicoId;
        Erro = erro;
        PayloadJson = payloadJson;
        OcorridoEm = DateTimeOffset.UtcNow;
    }
}
