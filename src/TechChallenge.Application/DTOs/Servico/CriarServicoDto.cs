using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.Servico;

public class CriarServicoDto
{
    [Required] [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Preco { get; set; }

    [Range(1, int.MaxValue)]
    public int TempoConclusaoMinutos { get; set; }
}
