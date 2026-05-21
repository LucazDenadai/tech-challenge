using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.UseCases.Usuario;

public record CriarUsuarioRequest(string Nome, string Email, string Senha, PerfilUsuario Perfil);
public record AtualizarUsuarioRequest(string Nome, string Email, string? Senha, PerfilUsuario Perfil);
