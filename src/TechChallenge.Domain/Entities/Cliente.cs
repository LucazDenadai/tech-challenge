namespace TechChallenge.Domain.Entities;

public class Cliente : EntityBase
{
    public string Nome { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public string Endereco { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    private readonly List<Veiculo> _veiculos = new();
    public IReadOnlyCollection<Veiculo> Veiculos => _veiculos.AsReadOnly();

    protected Cliente() { }

    public Cliente(string nome, string cpf, string email, string telefone, string endereco)
    {
        Nome = nome;
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
        Endereco = endereco;
    }

    public void Atualizar(string nome, string email, string telefone, string endereco)
    {
        Nome = nome;
        Email = email;
        Telefone = telefone;
        Endereco = endereco;
        MarcarAtualizado();
    }

    public void Desativar()
    {
        Ativo = false;
        MarcarAtualizado();
    }
}
