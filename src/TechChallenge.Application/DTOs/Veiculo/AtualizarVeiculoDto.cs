using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.Veiculo;

public class AtualizarVeiculoDto
{
    [Required]
    [MaxLength(50)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Modelo { get; set; } = string.Empty;

    public int Ano { get; set; }

    [Required]
    [MaxLength(30)]
    public string Cor { get; set; } = string.Empty;
}
