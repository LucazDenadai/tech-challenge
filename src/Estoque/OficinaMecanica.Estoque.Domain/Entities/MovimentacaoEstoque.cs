using System.Diagnostics.CodeAnalysis;
using OficinaMecanica.Estoque.Domain.Enums;

namespace OficinaMecanica.Estoque.Domain.Entities;

public class MovimentacaoEstoque : EntityBase
{
    public Guid PecaId { get; private set; }
    public TipoMovimentacao Tipo { get; private set; }
    public int Quantidade { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public Guid? OsId { get; private set; }
    public DateTime OcorridoEm { get; private set; }

    [ExcludeFromCodeCoverage]
    protected MovimentacaoEstoque() { }

    public MovimentacaoEstoque(Guid pecaId, TipoMovimentacao tipo, int quantidade, string motivo, Guid? osId = null)
    {
        PecaId = pecaId;
        Tipo = tipo;
        Quantidade = quantidade;
        Motivo = motivo;
        OsId = osId;
        OcorridoEm = DateTime.UtcNow;
    }
}
