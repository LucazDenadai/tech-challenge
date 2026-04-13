using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Application.DTOs.Usuario;
using TechChallenge.Application.Interfaces;

namespace TechChallenge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service) => _service = service;

    /// <summary>Listar todos os usuários</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<UsuarioDto>), 200)]
    public async Task<IActionResult> ObterTodos() => Ok(await _service.ObterTodosAsync());

    /// <summary>Obter usuário por ID</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UsuarioDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _service.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Criar novo usuário</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UsuarioDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioDto dto)
    {
        var criado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualizar usuário</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UsuarioDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarUsuarioDto dto)
    {
        return Ok(await _service.AtualizarAsync(id, dto));
    }

    /// <summary>Desativar usuário</summary>
    [HttpPatch("{id:guid}/desativar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Desativar(Guid id)
    {
        await _service.DesativarAsync(id);
        return NoContent();
    }
}
