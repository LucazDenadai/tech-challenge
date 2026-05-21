using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.Usuario;

public record UsuarioResponse(Guid Id, string Nome, string Email, PerfilUsuario Perfil, bool Ativo);
