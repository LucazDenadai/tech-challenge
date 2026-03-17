using TechChallenge.Application.DTOs.Peca;

namespace TechChallenge.Application.Interfaces;

public interface IPecaService
{
    Task<IEnumerable<PecaDto>> ObterTodosAsync();
    Task<PecaDto?> ObterPorIdAsync(Guid id);
    Task<PecaDto> CriarAsync(CriarPecaDto dto);
    Task<PecaDto> AtualizarAsync(Guid id, CriarPecaDto dto);
    Task DesativarAsync(Guid id);
}
