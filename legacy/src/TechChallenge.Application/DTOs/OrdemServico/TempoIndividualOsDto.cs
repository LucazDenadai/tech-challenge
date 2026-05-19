namespace TechChallenge.Application.DTOs.OrdemServico;

public class TempoIndividualOsDto
{
    public string Numero { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public double TempoTotalHoras { get; set; }
    public string TempoTotalFormatado { get; set; } = string.Empty;
    public string Observacao { get; set; } = string.Empty;
    public List<TempoPorStatusDto> TemposPorStatus { get; set; } = new();
}
