namespace TechChallenge.Domain.Entities;

public class Peca : EntityBase
{
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int QuantidadeEstoque { get; private set; }
    public bool Ativo { get; private set; } = true;

    protected Peca() { }

    public Peca(string nome, string descricao, decimal preco, int quantidadeEstoque)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        QuantidadeEstoque = quantidadeEstoque;
    }

    public void Atualizar(string nome, string descricao, decimal preco)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        MarcarAtualizado();
    }

    public void AdicionarEstoque(int quantidade)
    {
        if (quantidade <= 0) throw new InvalidOperationException("Quantidade deve ser positiva.");
        QuantidadeEstoque += quantidade;
        MarcarAtualizado();
    }

    public void ConsumirEstoque(int quantidade)
    {
        if (quantidade <= 0) throw new InvalidOperationException("Quantidade deve ser positiva.");
        if (QuantidadeEstoque < quantidade) throw new InvalidOperationException("Estoque insuficiente.");
        QuantidadeEstoque -= quantidade;
        MarcarAtualizado();
    }

    public void Desativar()
    {
        Ativo = false;
        MarcarAtualizado();
    }
}
