namespace TechChallenge.Domain.Entities;

public class ItemPeca : EntityBase
{
    public Guid OrdemServicoId { get; private set; }
    public Guid PecaId { get; private set; }
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal => Quantidade * ValorUnitario;

    public Peca? Peca { get; protected set; }
    public OrdemServico? OrdemServico { get; protected set; }

    protected ItemPeca() { }

    public ItemPeca(Guid ordemServicoId, Guid pecaId, int quantidade, decimal valorUnitario)
    {
        OrdemServicoId = ordemServicoId;
        PecaId = pecaId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }
}
