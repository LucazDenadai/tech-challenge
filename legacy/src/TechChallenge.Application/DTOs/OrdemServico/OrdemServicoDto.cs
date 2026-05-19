using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.DTOs.OrdemServico;

public class OrdemServicoDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public Guid VeiculoId { get; set; }
    public string PlacaVeiculo { get; set; } = string.Empty;
    public string MarcaVeiculo { get; set; } = string.Empty;
    public string ModeloVeiculo { get; set; } = string.Empty;
    public int AnoVeiculo { get; set; }
    public StatusOrdemServico Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public decimal ValorTotal { get; set; }
    public List<ItemServicoDto> ItensServico { get; set; } = new();
    public List<ItemPecaDto> ItensPeca { get; set; } = new();
    public List<HistoricoStatusDto> Historico { get; set; } = new();
}

public class ItemServicoDto
{
    public Guid Id { get; set; }
    public Guid ServicoId { get; set; }
    public string NomeServico { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}

public class ItemPecaDto
{
    public Guid Id { get; set; }
    public Guid PecaId { get; set; }
    public string NomePeca { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}

public class HistoricoStatusDto
{
    public StatusOrdemServico StatusAnterior { get; set; }
    public StatusOrdemServico StatusNovo { get; set; }
    public DateTime DataAlteracao { get; set; }
}
