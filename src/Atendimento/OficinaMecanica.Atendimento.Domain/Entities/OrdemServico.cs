using System.Diagnostics.CodeAnalysis;
using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Domain.Entities;

public class OrdemServico : EntityBase
{
    public string Numero { get; private set; } = string.Empty;
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public StatusOrdemServico Status { get; private set; } = StatusOrdemServico.Recebida;
    public string Observacoes { get; private set; } = string.Empty;
    public DateTime DataAbertura { get; private set; } = DateTime.UtcNow;
    public DateTime? DataFechamento { get; private set; }

    public Cliente? Cliente { get; protected set; }
    public Veiculo? Veiculo { get; protected set; }

    private readonly List<ItemServico> _itensServico = new();
    public IReadOnlyCollection<ItemServico> ItensServico => _itensServico.AsReadOnly();

    private readonly List<ItemPeca> _itensPeca = new();
    public IReadOnlyCollection<ItemPeca> ItensPeca => _itensPeca.AsReadOnly();

    private readonly List<HistoricoStatusOS> _historico = new();
    public IReadOnlyCollection<HistoricoStatusOS> Historico => _historico.AsReadOnly();

    public decimal ValorTotal => _itensServico.Sum(i => i.ValorTotal) + _itensPeca.Sum(i => i.ValorTotal);

    [ExcludeFromCodeCoverage]
    protected OrdemServico() { }

    public OrdemServico(string numero, Guid clienteId, Guid veiculoId, string observacoes)
    {
        Numero = numero;
        ClienteId = clienteId;
        VeiculoId = veiculoId;
        Observacoes = observacoes;
    }

    public void AlterarStatus(StatusOrdemServico novoStatus)
    {
        if (Status is StatusOrdemServico.Entregue or StatusOrdemServico.Cancelada)
            throw new InvalidOperationException($"OS no status '{Status}' não pode ser alterada.");

        var statusAnterior = Status;

        if (novoStatus == StatusOrdemServico.Cancelada)
        {
            Status = StatusOrdemServico.Cancelada;
            DataFechamento = DateTime.UtcNow;
            _historico.Add(new HistoricoStatusOS(Id, statusAnterior, Status));
            MarcarAtualizado();
            return;
        }

        var proximoEsperado = Status switch
        {
            StatusOrdemServico.Recebida => StatusOrdemServico.EmDiagnostico,
            StatusOrdemServico.EmDiagnostico => StatusOrdemServico.AguardandoAprovacao,
            StatusOrdemServico.AguardandoAprovacao => StatusOrdemServico.EmExecucao,
            StatusOrdemServico.EmExecucao => StatusOrdemServico.Finalizada,
            StatusOrdemServico.Finalizada => StatusOrdemServico.Entregue,
            _ => throw new InvalidOperationException("Status inválido.")
        };

        if (novoStatus != proximoEsperado)
            throw new InvalidOperationException($"O próximo status esperado é '{proximoEsperado}'.");

        Status = novoStatus;

        if (Status is StatusOrdemServico.Finalizada or StatusOrdemServico.Entregue)
            DataFechamento = DateTime.UtcNow;

        _historico.Add(new HistoricoStatusOS(Id, statusAnterior, Status));
        MarcarAtualizado();
    }

    public void AdicionarItemServico(ItemServico item)
    {
        if (Status != StatusOrdemServico.EmDiagnostico && Status != StatusOrdemServico.EmExecucao)
            throw new InvalidOperationException($"Não é possível adicionar serviços com a OS no status '{Status}'.");
        if (_itensServico.Any(i => i.ServicoId == item.ServicoId))
            throw new InvalidOperationException($"O serviço já foi adicionado a esta OS.");
        _itensServico.Add(item);
    }

    public void AdicionarItemPeca(ItemPeca item)
    {
        if (Status != StatusOrdemServico.EmDiagnostico && Status != StatusOrdemServico.EmExecucao)
            throw new InvalidOperationException($"Não é possível adicionar peças com a OS no status '{Status}'.");
        _itensPeca.Add(item);
    }

    public ItemServico RemoverItemServico(Guid itemId)
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
            throw new InvalidOperationException($"Itens só podem ser cancelados com a OS em diagnóstico. Status atual: '{Status}'.");
        var item = _itensServico.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException("Item de serviço não encontrado nesta OS.");
        _itensServico.Remove(item);
        return item;
    }

    public ItemPeca RemoverItemPeca(Guid itemId)
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
            throw new InvalidOperationException($"Itens só podem ser cancelados com a OS em diagnóstico. Status atual: '{Status}'.");
        var item = _itensPeca.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException("Item de peça não encontrado nesta OS.");
        _itensPeca.Remove(item);
        return item;
    }

    public void AtualizarObservacoes(string observacoes)
    {
        Observacoes = observacoes;
        MarcarAtualizado();
    }
}
