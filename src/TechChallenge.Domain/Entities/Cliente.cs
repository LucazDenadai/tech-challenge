using System.Diagnostics.CodeAnalysis;
using TechChallenge.Domain.Validators;

namespace TechChallenge.Domain.Entities;

public class Cliente : EntityBase
{
    public string Nome { get; private set; } = string.Empty;
    public string Documento { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public string Endereco { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    private readonly List<Veiculo> _veiculos = new();
    [ExcludeFromCodeCoverage]
    public IReadOnlyCollection<Veiculo> Veiculos => _veiculos.AsReadOnly();

    [ExcludeFromCodeCoverage]
    protected Cliente() { }

    public Cliente(string nome, string documento, string email, string telefone, string endereco)
    {
        ValidarDocumento(documento);
        Nome = nome;
        Documento = documento;
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

    private static void ValidarDocumento(string documento)
    {
        var digits = documento?.Replace(".", "").Replace("-", "").Replace("/", "").Trim() ?? "";
        if (digits.Length == 11)
            CpfValidator.Validar(documento!);
        else if (digits.Length == 14)
            CnpjValidator.Validar(documento!);
        else
            throw new ArgumentException("Documento inválido. Informe um CPF (11 dígitos) ou CNPJ (14 dígitos).", nameof(documento));
    }
}
