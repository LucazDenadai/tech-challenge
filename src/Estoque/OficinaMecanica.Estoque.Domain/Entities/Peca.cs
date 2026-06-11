using System.Diagnostics.CodeAnalysis;

namespace OficinaMecanica.Estoque.Domain.Entities;

public class Peca : EntityBase
{
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public int QuantidadeEstoque { get; private set; }

    [ExcludeFromCodeCoverage]
    protected Peca() { }

    public Peca(string nome, string descricao, decimal valor, int quantidadeEstoque)
    {
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
        QuantidadeEstoque = quantidadeEstoque;
    }

    public void Atualizar(string nome, string descricao, decimal valor)
    {
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
        MarcarAtualizado();
    }

    public void SubtrairEstoque(int quantidade)
    {
        if (quantidade > QuantidadeEstoque)
            throw new InvalidOperationException("Quantidade solicitada maior que o estoque disponível.");

        QuantidadeEstoque -= quantidade;
        MarcarAtualizado();
    }

    public void AdicionarEstoque(int quantidade)
    {
        QuantidadeEstoque += quantidade;
        MarcarAtualizado();
    }
}
