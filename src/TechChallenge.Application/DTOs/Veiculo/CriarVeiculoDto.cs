using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.Veiculo;

public class CriarVeiculoDto
{
    [Required]
    public Guid ClienteId { get; set; }

    [Required] [MaxLength(10)]
    public string Placa { get; set; } = string.Empty;

    [Required] [MaxLength(50)]
    public string Marca { get; set; } = string.Empty;

    [Required] [MaxLength(50)]
    public string Modelo { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Ano { get; set; }

    [Required] [MaxLength(30)]
    public string Cor { get; set; } = string.Empty;
}
