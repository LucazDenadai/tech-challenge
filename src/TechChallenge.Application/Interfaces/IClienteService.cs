using TechChallenge.Application.DTOs.Cliente;

namespace TechChallenge.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> ObterTodosAsync();
    Task<ClienteDto?> ObterPorIdAsync(Guid id);
    Task<ClienteDto> CriarAsync(CriarClienteDto dto);
    Task<ClienteDto> AtualizarAsync(Guid id, CriarClienteDto dto);
    Task DesativarAsync(Guid id);
}
