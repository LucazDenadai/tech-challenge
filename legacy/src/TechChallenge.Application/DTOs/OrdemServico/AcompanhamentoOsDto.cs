using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.DTOs.OrdemServico;

public class AcompanhamentoOsDto
{
    public string Numero { get; set; } = string.Empty;
    public StatusOrdemServico Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public string PlacaVeiculo { get; set; } = string.Empty;
    public string MarcaVeiculo { get; set; } = string.Empty;
    public string ModeloVeiculo { get; set; } = string.Empty;
    public int AnoVeiculo { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public decimal ValorTotal { get; set; }
    public List<ItemServicoDto> ItensServico { get; set; } = new();
    public List<ItemPecaDto> ItensPeca { get; set; } = new();
    public List<HistoricoStatusDto> Historico { get; set; } = new();
}
