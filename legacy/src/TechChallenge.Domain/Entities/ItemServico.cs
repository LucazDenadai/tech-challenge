using System.Diagnostics.CodeAnalysis;

namespace TechChallenge.Domain.Entities;

public class ItemServico : EntityBase
{
    public Guid OrdemServicoId { get; private set; }
    public Guid ServicoId { get; private set; }
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal => Quantidade * ValorUnitario;

    public Servico? Servico { get; protected set; }
    [ExcludeFromCodeCoverage]
    public OrdemServico? OrdemServico { get; protected set; }

    [ExcludeFromCodeCoverage]
    protected ItemServico() { }

    public ItemServico(Guid ordemServicoId, Guid servicoId, int quantidade, decimal valorUnitario)
    {
        OrdemServicoId = ordemServicoId;
        ServicoId = servicoId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }
}
