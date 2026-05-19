using TechChallenge.Application.DTOs.Servico;

namespace TechChallenge.Application.Interfaces;

public interface IServicoService
{
    Task<IEnumerable<ServicoDto>> ObterTodosAsync();
    Task<ServicoDto?> ObterPorIdAsync(Guid id);
    Task<ServicoDto> CriarAsync(CriarServicoDto dto);
    Task<ServicoDto> AtualizarAsync(Guid id, CriarServicoDto dto);
    Task DesativarAsync(Guid id);
}
