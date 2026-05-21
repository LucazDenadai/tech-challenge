using System.ComponentModel.DataAnnotations;
using OficinaMecanica.Atendimento.Application.Validators;
using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.Usuario;

public record CriarUsuarioRequest(
    [property: Required][property: MaxLength(100)] string Nome,
    [property: Required][property: EmailAddress] string Email,
    [property: Required][property: SenhaForte] string Senha,
    [property: Required] PerfilUsuario Perfil);

public record AtualizarUsuarioRequest(
    [property: Required][property: MaxLength(100)] string Nome,
    [property: Required][property: EmailAddress] string Email,
    [property: SenhaForte] string? Senha,
    [property: Required] PerfilUsuario Perfil);
