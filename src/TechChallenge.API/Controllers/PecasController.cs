using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Application.DTOs.Peca;
using TechChallenge.Application.Interfaces;

namespace TechChallenge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class PecasController : ControllerBase
{
    private readonly IPecaService _service;

    public PecasController(IPecaService service) => _service = service;

    /// <summary>Listar todas as peças</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PecaDto>), 200)]
    public async Task<IActionResult> ObterTodos() => Ok(await _service.ObterTodosAsync());

    /// <summary>Obter peça por ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PecaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _service.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Criar nova peça</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PecaDto), 201)]
    public async Task<IActionResult> Criar([FromBody] CriarPecaDto dto)
    {
        var criado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualizar peça</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PecaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarPecaDto dto)
    {
        try { return Ok(await _service.AtualizarAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Desativar peça</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Desativar(Guid id)
    {
        try { await _service.DesativarAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
