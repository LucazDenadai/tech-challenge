using TechChallenge.Application.DTOs.Veiculo;
using TechChallenge.Application.Interfaces;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;

namespace TechChallenge.Application.Services;

public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _repo;
    private readonly IClienteRepository _clienteRepo;

    public VeiculoService(IVeiculoRepository repo, IClienteRepository clienteRepo)
    {
        _repo = repo;
        _clienteRepo = clienteRepo;
    }

    public async Task<IEnumerable<VeiculoDto>> ObterTodosAsync()
        => (await _repo.ObterTodosAsync()).Select(MapDto);

    public async Task<VeiculoDto?> ObterPorIdAsync(Guid id)
    {
        var e = await _repo.ObterPorIdAsync(id);
        return e is null ? null : MapDto(e);
    }

    public async Task<IEnumerable<VeiculoDto>> ObterPorClienteAsync(Guid clienteId)
        => (await _repo.ObterPorClienteAsync(clienteId)).Select(MapDto);

    public async Task<VeiculoDto> CriarAsync(CriarVeiculoDto dto)
    {
        var cliente = await _clienteRepo.ObterPorIdAsync(dto.ClienteId)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");
        var veiculo = new Veiculo(dto.ClienteId, dto.Placa, dto.Marca, dto.Modelo, dto.Ano, dto.Cor);
        await _repo.AdicionarAsync(veiculo);
        await _repo.SalvarAsync();
        var veiculoDto = MapDto(veiculo);
        veiculoDto.NomeCliente = cliente.Nome;
        return veiculoDto;
    }

    public async Task<VeiculoDto> AtualizarAsync(Guid id, CriarVeiculoDto dto)
    {
        var veiculo = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Veículo não encontrado.");
        veiculo.Atualizar(dto.Marca, dto.Modelo, dto.Ano, dto.Cor);
        await _repo.AtualizarAsync(veiculo);
        await _repo.SalvarAsync();
        return MapDto(veiculo);
    }

    public async Task RemoverAsync(Guid id)
    {
        var veiculo = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Veículo não encontrado.");
        await _repo.RemoverAsync(veiculo);
        await _repo.SalvarAsync();
    }

    private static VeiculoDto MapDto(Veiculo v) => new()
    {
        Id = v.Id, ClienteId = v.ClienteId, NomeCliente = v.Cliente?.Nome ?? "",
        Placa = v.Placa, Marca = v.Marca, Modelo = v.Modelo, Ano = v.Ano, Cor = v.Cor
    };
}
