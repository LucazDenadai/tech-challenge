using TechChallenge.Application.DTOs.Usuario;

namespace TechChallenge.Application.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> ObterTodosAsync();
    Task<IEnumerable<UsuarioDto>> BuscarPorEmailAsync(string email);
    Task<UsuarioDto?> ObterPorIdAsync(Guid id);
    Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto);
    Task<UsuarioDto> AtualizarAsync(Guid id, CriarUsuarioDto dto);
    Task DesativarAsync(Guid id);
}
