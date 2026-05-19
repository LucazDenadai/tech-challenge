using System.ComponentModel.DataAnnotations;
using TechChallenge.Application.Validators;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.DTOs.Usuario;

public class CriarUsuarioDto
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [SenhaForte]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public PerfilUsuario Perfil { get; set; }
}
