using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.Cliente;

public class CriarClienteDto
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF (11 dígitos) ou CNPJ (14 dígitos), com ou sem máscara.</summary>
    [Required]
    [StringLength(18, MinimumLength = 11)]
    public string Documento { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Endereco { get; set; } = string.Empty;
}
