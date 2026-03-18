using TechChallenge.Application.DTOs.Peca;
using TechChallenge.Application.Interfaces;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;

namespace TechChallenge.Application.Services;

public class PecaService : IPecaService
{
    private readonly IPecaRepository _repo;

    public PecaService(IPecaRepository repo) => _repo = repo;

    public async Task<IEnumerable<PecaDto>> ObterTodosAsync()
        => (await _repo.ObterTodosAsync()).Select(MapDto);

    public async Task<PecaDto?> ObterPorIdAsync(Guid id)
    {
        var e = await _repo.ObterPorIdAsync(id);
        return e is null ? null : MapDto(e);
    }

    public async Task<PecaDto> CriarAsync(CriarPecaDto dto)
    {
        var peca = new Peca(dto.Nome, dto.Descricao, dto.Preco, dto.QuantidadeEstoque);
        await _repo.AdicionarAsync(peca);
        await _repo.SalvarAsync();
        return MapDto(peca);
    }

    public async Task<PecaDto> AtualizarAsync(Guid id, CriarPecaDto dto)
    {
        var peca = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Peça não encontrada.");
        peca.Atualizar(dto.Nome, dto.Descricao, dto.Preco);
        if (dto.QuantidadeEstoque > 0) peca.AdicionarEstoque(dto.QuantidadeEstoque);
        await _repo.AtualizarAsync(peca);
        await _repo.SalvarAsync();
        return MapDto(peca);
    }

    public async Task DesativarAsync(Guid id)
    {
        var peca = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Peça não encontrada.");
        peca.Desativar();
        await _repo.AtualizarAsync(peca);
        await _repo.SalvarAsync();
    }

    private static PecaDto MapDto(Peca p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Descricao = p.Descricao,
        Preco = p.Preco,
        QuantidadeEstoque = p.QuantidadeEstoque,
        Ativo = p.Ativo
    };
}
