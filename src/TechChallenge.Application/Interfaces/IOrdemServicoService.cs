using TechChallenge.Application.DTOs.OrdemServico;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Interfaces;

public interface IOrdemServicoService
{
    Task<IEnumerable<OrdemServicoDto>> ObterTodosAsync();
    Task<OrdemServicoDto?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<OrdemServicoDto>> ObterPorClienteAsync(Guid clienteId);
    Task<IEnumerable<OrdemServicoDto>> ObterPorStatusAsync(StatusOrdemServico status);
    Task<IEnumerable<OrdemServicoDto>> FiltrarAsync(Guid? clienteId, StatusOrdemServico? status);
    Task<OrdemServicoDto> CriarAsync(CriarOrdemServicoDto dto);
    Task<OrdemServicoDto> AlterarStatusAsync(Guid id, AlterarStatusDto dto);
    Task<OrdemServicoDto> AdicionarItemAsync(Guid ordemId, AdicionarItemDto dto);
    Task<OrdemServicoDto> CancelarItemAsync(Guid ordemId, Guid itemId);
}
