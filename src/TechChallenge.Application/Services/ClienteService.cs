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
        => (await _repo.ObterTodosComDetalhesAsync()).Select(r => MapDto(r.Cliente, r.TotalOrdens));

    public async Task<IEnumerable<ClienteDto>> BuscarAsync(string termo)
        => (await _repo.BuscarComDetalhesAsync(termo)).Select(r => MapDto(r.Cliente, r.TotalOrdens));

    public async Task<ClienteDto?> ObterPorIdAsync(Guid id)
    {
        var e = await _repo.ObterPorIdAsync(id);
        return e is null ? null : MapDto(e);
    }

    public async Task<ClienteDto> CriarAsync(CriarClienteDto dto)
    {
        if (await _repo.DocumentoExisteAsync(dto.Documento))
            throw new InvalidOperationException("CPF/CNPJ já cadastrado.");
        var cliente = new Cliente(dto.Nome, dto.Documento, dto.Email, dto.Telefone, dto.Endereco);
        await _repo.AdicionarAsync(cliente);
        await _repo.SalvarAsync();
        return MapDto(cliente);
    }

    public async Task<ClienteDto> AtualizarAsync(Guid id, CriarClienteDto dto)
    {
        var cliente = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");
        if (await _repo.DocumentoExisteAsync(dto.Documento, id))
            throw new InvalidOperationException("CPF/CNPJ já cadastrado.");
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

    private static ClienteDto MapDto(Cliente c, int totalOrdens = 0) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Documento = c.Documento,
        Email = c.Email,
        Telefone = c.Telefone,
        Endereco = c.Endereco,
        Ativo = c.Ativo,
        CriadoEm = c.CriadoEm,
        TotalOrdensServico = totalOrdens,
        Veiculos = c.Veiculos.Select(v => new VeiculoResumoDto
        {
            Id = v.Id,
            Placa = v.Placa,
            Marca = v.Marca,
            Modelo = v.Modelo,
            Ano = v.Ano,
            Cor = v.Cor
        })
    };
}
