using TechChallenge.Application.DTOs.Servico;
using TechChallenge.Application.Interfaces;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;

namespace TechChallenge.Application.Services;

public class ServicoService : IServicoService
{
    private readonly IServicoRepository _repo;

    public ServicoService(IServicoRepository repo) => _repo = repo;

    public async Task<IEnumerable<ServicoDto>> ObterTodosAsync()
        => (await _repo.ObterAtivosAsync()).Select(MapDto);

    public async Task<ServicoDto?> ObterPorIdAsync(Guid id)
    {
        var e = await _repo.ObterPorIdAsync(id);
        return e is null ? null : MapDto(e);
    }

    public async Task<ServicoDto> CriarAsync(CriarServicoDto dto)
    {
        var servico = new Servico(dto.Nome, dto.Descricao, dto.Preco, dto.TempoConclusaoMinutos);
        await _repo.AdicionarAsync(servico);
        await _repo.SalvarAsync();
        return MapDto(servico);
    }

    public async Task<ServicoDto> AtualizarAsync(Guid id, CriarServicoDto dto)
    {
        var servico = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Serviço não encontrado.");
        servico.Atualizar(dto.Nome, dto.Descricao, dto.Preco, dto.TempoConclusaoMinutos);
        await _repo.AtualizarAsync(servico);
        await _repo.SalvarAsync();
        return MapDto(servico);
    }

    public async Task DesativarAsync(Guid id)
    {
        var servico = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Serviço não encontrado.");
        servico.Desativar();
        await _repo.AtualizarAsync(servico);
        await _repo.SalvarAsync();
    }

    private static ServicoDto MapDto(Servico s) => new()
    {
        Id = s.Id,
        Nome = s.Nome,
        Descricao = s.Descricao,
        Preco = s.Preco,
        TempoConclusaoMinutos = s.TempoConclusaoMinutos,
        Ativo = s.Ativo
    };
}
