using System.Diagnostics.CodeAnalysis;

namespace OficinaMecanica.Atendimento.Domain.Entities;

// Entidade de referência — a gestão de estoque pertence ao microserviço Estoque.
// Mantida aqui apenas para EF Core mapear a FK de ItemPeca.
public class Peca : EntityBase
{
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public bool Ativo { get; private set; } = true;

    [ExcludeFromCodeCoverage]
    protected Peca() { }

    public Peca(string nome, string descricao, decimal preco)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
    }
}
