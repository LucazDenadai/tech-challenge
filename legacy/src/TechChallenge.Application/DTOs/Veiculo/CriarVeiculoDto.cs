using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.Veiculo;

public class CriarVeiculoDto
{
    /// <summary>CPF (11 dígitos) ou CNPJ (14 dígitos) do cliente proprietário</summary>
    [Required]
    public string DocumentoCliente { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Placa { get; set; } = string.Empty;

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
