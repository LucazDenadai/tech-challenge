using TechChallenge.Application.DTOs.OrdemServico;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Interfaces;

public interface IOrdemServicoService
{
    Task<IEnumerable<OrdemServicoDto>> ObterTodosAsync();
    Task<OrdemServicoDto?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<OrdemServicoDto>> ObterPorClienteAsync(Guid clienteId);
    Task<IEnumerable<OrdemServicoDto>> ObterPorStatusAsync(StatusOrdemServico status);
    Task<IEnumerable<OrdemServicoDto>> FiltrarAsync(string? busca, StatusOrdemServico? status);
    Task<OrdemServicoDto> CriarAsync(CriarOrdemServicoDto dto);
    Task<OrdemServicoDto> AlterarStatusAsync(string numero, AlterarStatusDto dto);
    Task<OrdemServicoDto> AdicionarItemAsync(Guid ordemId, AdicionarItemDto dto);
    Task<OrdemServicoDto> CancelarItemAsync(Guid ordemId, Guid itemId);
    Task<AcompanhamentoOsDto?> AcompanharPorNumeroAsync(string numero);
    Task<TempoMedioExecucaoDto> ObterTempoMedioExecucaoAsync();
    Task<TempoIndividualOsDto?> ObterTempoIndividualAsync(string numero);
}
