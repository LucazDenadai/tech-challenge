using System.Diagnostics.CodeAnalysis;

namespace TechChallenge.Domain.Entities;

public class Servico : EntityBase
{
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int TempoConclusaoMinutos { get; private set; }
    public bool Ativo { get; private set; } = true;

    [ExcludeFromCodeCoverage]
    protected Servico() { }

    public Servico(string nome, string descricao, decimal preco, int tempoConclusaoMinutos)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        TempoConclusaoMinutos = tempoConclusaoMinutos;
    }

    public void Atualizar(string nome, string descricao, decimal preco, int tempoConclusaoMinutos)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        TempoConclusaoMinutos = tempoConclusaoMinutos;
        MarcarAtualizado();
    }

    public void Desativar()
    {
        Ativo = false;
        MarcarAtualizado();
    }
}
