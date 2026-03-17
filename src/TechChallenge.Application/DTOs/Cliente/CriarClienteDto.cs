using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.Cliente;

public class CriarClienteDto
{
    [Required] [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required] [StringLength(11, MinimumLength = 11)]
    public string Cpf { get; set; } = string.Empty;

    [Required] [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required] [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [Required] [MaxLength(200)]
    public string Endereco { get; set; } = string.Empty;
}
