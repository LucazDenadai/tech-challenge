using TechChallenge.Domain.Enums;

namespace TechChallenge.Domain.Entities;

public class OrdemServico : EntityBase
{
    public string Numero { get; private set; } = string.Empty;
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public StatusOrdemServico Status { get; private set; } = StatusOrdemServico.Recebida;
    public string Observacoes { get; private set; } = string.Empty;
    public DateTime DataAbertura { get; private set; } = DateTime.UtcNow;
    public DateTime? DataFechamento { get; private set; }

    public Cliente? Cliente { get; private set; }
    public Veiculo? Veiculo { get; private set; }

    private readonly List<ItemServico> _itensServico = new();
    public IReadOnlyCollection<ItemServico> ItensServico => _itensServico.AsReadOnly();

    private readonly List<ItemPeca> _itensPeca = new();
    public IReadOnlyCollection<ItemPeca> ItensPeca => _itensPeca.AsReadOnly();

    public decimal ValorTotal => _itensServico.Sum(i => i.ValorTotal) + _itensPeca.Sum(i => i.ValorTotal);

    protected OrdemServico() { }

    public OrdemServico(string numero, Guid clienteId, Guid veiculoId, string observacoes)
    {
        Numero = numero;
        ClienteId = clienteId;
        VeiculoId = veiculoId;
        Observacoes = observacoes;
    }

    public void AvancarStatus()
    {
        Status = Status switch
        {
            StatusOrdemServico.Recebida => StatusOrdemServico.EmDiagnostico,
            StatusOrdemServico.EmDiagnostico => StatusOrdemServico.AguardandoAprovacao,
            StatusOrdemServico.AguardandoAprovacao => StatusOrdemServico.EmExecucao,
            StatusOrdemServico.EmExecucao => StatusOrdemServico.Finalizada,
            StatusOrdemServico.Finalizada => StatusOrdemServico.Entregue,
            _ => throw new InvalidOperationException("OS já está no status final.")
        };

        if (Status == StatusOrdemServico.Finalizada || Status == StatusOrdemServico.Entregue)
            DataFechamento = DateTime.UtcNow;

        MarcarAtualizado();
    }

    public void AdicionarItemServico(ItemServico item) => _itensServico.Add(item);
    public void AdicionarItemPeca(ItemPeca item) => _itensPeca.Add(item);

    public void AtualizarObservacoes(string observacoes)
    {
        Observacoes = observacoes;
        MarcarAtualizado();
    }
}
