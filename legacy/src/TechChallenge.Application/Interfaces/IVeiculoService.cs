using TechChallenge.Application.DTOs.Veiculo;

namespace TechChallenge.Application.Interfaces;

public interface IVeiculoService
{
    Task<IEnumerable<VeiculoDto>> ObterTodosAsync();
    Task<VeiculoDto?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<VeiculoDto>> ObterPorClienteAsync(Guid clienteId);
    Task<IEnumerable<VeiculoDto>> ObterPorDocumentoClienteAsync(string documento);
    Task<VeiculoDto> CriarAsync(CriarVeiculoDto dto);
    Task<VeiculoDto> AtualizarAsync(Guid id, AtualizarVeiculoDto dto);
    Task RemoverAsync(Guid id);
}
