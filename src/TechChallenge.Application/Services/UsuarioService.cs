using TechChallenge.Application.DTOs.Usuario;
using TechChallenge.Application.Interfaces;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;

namespace TechChallenge.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repo;

    public UsuarioService(IUsuarioRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<UsuarioDto>> ObterTodosAsync()
    {
        var lista = await _repo.ObterTodosAsync();
        return lista.Select(MapDto);
    }

    public async Task<UsuarioDto?> ObterPorIdAsync(Guid id)
    {
        var entidade = await _repo.ObterPorIdAsync(id);
        return entidade is null ? null : MapDto(entidade);
    }

    public async Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto)
    {
        if (await _repo.EmailExisteAsync(dto.Email))
            throw new InvalidOperationException("E-mail já cadastrado.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
        var usuario = new Usuario(dto.Nome, dto.Email, senhaHash, dto.Perfil);
        await _repo.AdicionarAsync(usuario);
        await _repo.SalvarAsync();
        return MapDto(usuario);
    }

    public async Task<UsuarioDto> AtualizarAsync(Guid id, CriarUsuarioDto dto)
    {
        var usuario = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (await _repo.EmailExisteAsync(dto.Email, id))
            throw new InvalidOperationException("E-mail já cadastrado.");

        usuario.Atualizar(dto.Nome, dto.Email, dto.Perfil);
        if (!string.IsNullOrEmpty(dto.Senha))
            usuario.AlterarSenha(BCrypt.Net.BCrypt.HashPassword(dto.Senha));

        await _repo.AtualizarAsync(usuario);
        await _repo.SalvarAsync();
        return MapDto(usuario);
    }

    public async Task DesativarAsync(Guid id)
    {
        var usuario = await _repo.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");
        usuario.Desativar();
        await _repo.AtualizarAsync(usuario);
        await _repo.SalvarAsync();
    }

    private static UsuarioDto MapDto(Usuario u) => new()
    {
        Id = u.Id,
        Nome = u.Nome,
        Email = u.Email,
        Perfil = u.Perfil,
        Ativo = u.Ativo,
        CriadoEm = u.CriadoEm
    };
}
