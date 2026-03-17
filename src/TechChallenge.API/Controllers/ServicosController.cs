using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Application.DTOs.Servico;
using TechChallenge.Application.Interfaces;

namespace TechChallenge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ServicosController : ControllerBase
{
    private readonly IServicoService _service;

    public ServicosController(IServicoService service) => _service = service;

    /// <summary>Listar todos os serviços</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServicoDto>), 200)]
    public async Task<IActionResult> ObterTodos() => Ok(await _service.ObterTodosAsync());

    /// <summary>Obter serviço por ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServicoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _service.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Criar novo serviço</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServicoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarServicoDto dto)
    {
        var criado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualizar serviço</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServicoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarServicoDto dto)
    {
        try { return Ok(await _service.AtualizarAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Desativar serviço</summary>
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
