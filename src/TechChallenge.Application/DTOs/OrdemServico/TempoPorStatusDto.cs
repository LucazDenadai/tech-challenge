namespace TechChallenge.Application.DTOs.OrdemServico;

public class TempoPorStatusDto
{
    public string Status { get; set; } = string.Empty;
    public double TempoHoras { get; set; }
    public string TempoFormatado { get; set; } = string.Empty;
}
