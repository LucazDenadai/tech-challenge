using TechChallenge.Application.DTOs.OrdemServico;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Interfaces;

public interface IOrdemServicoService
{
    Task<IEnumerable<OrdemServicoDto>> ObterTodosAsync();
    Task<OrdemServicoDto?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<OrdemServicoDto>> ObterPorClienteAsync(Guid clienteId);
    Task<IEnumerable<OrdemServicoDto>> ObterPorStatusAsync(StatusOrdemServico status);
    Task<OrdemServicoDto> CriarAsync(CriarOrdemServicoDto dto);
    Task<OrdemServicoDto> AvancarStatusAsync(Guid id);
    Task<OrdemServicoDto> AdicionarItemServicoAsync(Guid ordemId, AdicionarItemServicoDto dto);
    Task<OrdemServicoDto> AdicionarItemPecaAsync(Guid ordemId, AdicionarItemPecaDto dto);
}
