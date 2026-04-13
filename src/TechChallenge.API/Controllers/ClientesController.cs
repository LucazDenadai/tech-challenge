using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Application.DTOs.Cliente;
using TechChallenge.Application.Interfaces;

namespace TechChallenge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service) => _service = service;

    /// <summary>Listar todos os clientes ou buscar por nome, CPF ou e-mail</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteDto>), 200)]
    public async Task<IActionResult> ObterTodos([FromQuery] string? busca)
    {
        if (!string.IsNullOrWhiteSpace(busca))
        {
            return Ok(await _service.BuscarAsync(busca));
        }
        return Ok(await _service.ObterTodosAsync());
    }

    /// <summary>Obter cliente por ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _service.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Criar novo cliente</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarClienteDto dto)
    {
        var criado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualizar cliente</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClienteDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarClienteDto dto)
    {
        return Ok(await _service.AtualizarAsync(id, dto));
    }

    /// <summary>Desativar cliente</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Desativar(Guid id)
    {
        await _service.DesativarAsync(id);
        return NoContent();
    }
}
