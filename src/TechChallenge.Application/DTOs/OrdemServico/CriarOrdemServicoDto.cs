using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.OrdemServico;

public class CriarOrdemServicoDto
{
    [Required]
    public Guid ClienteId { get; set; }

    [Required]
    public Guid VeiculoId { get; set; }

    [MaxLength(500)]
    public string Observacoes { get; set; } = string.Empty;
}
