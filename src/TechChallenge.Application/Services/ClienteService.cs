using TechChallenge.Application.DTOs.Cliente;
using TechChallenge.Application.Interfaces;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;

namespace TechChallenge.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repo;

    public ClienteService(IClienteRepository repo) => _repo = repo;

    public async Task<IEnumerable<ClienteDto>> ObterTodosAsync()
        => (await _repo.ObterTodosAsync()).Select(MapDto);

    public async Task<ClienteDto?> ObterPorIdAsync(Guid id)
    {
        var e = await _repo.ObterPorIdAsync(id);
        return e is null ? null : MapDto(e);
    }

    public async Task<ClienteDto> CriarAsync(CriarClienteDto dto)
    {
        if (await _repo.CpfExisteAsync(dto.Cpf))
            throw new InvalidOperationException("CPF já cadastrado.");
        var cliente = new Cliente(dto.Nome, dto.Cpf, dto.Email, dto.Telefone, dto.Endereco);
        await _repo.AdicionarAsync(cliente);
        await _repo.SalvarAsync();
        return MapDto(cliente);
    }

    public async Task<ClienteDto> AtualizarAsync(Guid id, CriarClienteDto dto)
    {
        var cliente = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");
        if (await _repo.CpfExisteAsync(dto.Cpf, id))
            throw new InvalidOperationException("CPF já cadastrado.");
        cliente.Atualizar(dto.Nome, dto.Email, dto.Telefone, dto.Endereco);
        await _repo.AtualizarAsync(cliente);
        await _repo.SalvarAsync();
        return MapDto(cliente);
    }

    public async Task DesativarAsync(Guid id)
    {
        var cliente = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");
        cliente.Desativar();
        await _repo.AtualizarAsync(cliente);
        await _repo.SalvarAsync();
    }

    private static ClienteDto MapDto(Cliente c) => new()
    {
        Id = c.Id, Nome = c.Nome, Cpf = c.Cpf, Email = c.Email,
        Telefone = c.Telefone, Endereco = c.Endereco, Ativo = c.Ativo, CriadoEm = c.CriadoEm
    };
}
