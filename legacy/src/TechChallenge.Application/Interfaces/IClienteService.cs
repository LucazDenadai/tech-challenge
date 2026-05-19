using TechChallenge.Application.DTOs.Cliente;

namespace TechChallenge.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> ObterTodosAsync();
    Task<IEnumerable<ClienteDto>> BuscarAsync(string termo);
    Task<ClienteDto?> ObterPorIdAsync(Guid id);
    Task<ClienteDto> CriarAsync(CriarClienteDto dto);
    Task<ClienteDto> AtualizarAsync(Guid id, AtualizarClienteDto dto);
    Task DesativarAsync(Guid id);
}
