using System.Diagnostics.CodeAnalysis;

namespace OficinaMecanica.Atendimento.Domain.Entities;

public class ItemPeca : EntityBase
{
    public Guid OrdemServicoId { get; private set; }
    public Guid PecaId { get; private set; }
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal => Quantidade * ValorUnitario;

    [ExcludeFromCodeCoverage]
    public OrdemServico? OrdemServico { get; protected set; }

    [ExcludeFromCodeCoverage]
    protected ItemPeca() { }

    public ItemPeca(Guid ordemServicoId, Guid pecaId, int quantidade, decimal valorUnitario)
    {
        OrdemServicoId = ordemServicoId;
        PecaId = pecaId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }
}
