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

    /// <summary>Listar todos os clientes ativos ou buscar por nome, CPF/CNPJ ou e-mail</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Atendente")]
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
    [Authorize(Roles = "Admin,Atendente")]
    [ProducesResponseType(typeof(ClienteDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _service.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>
    /// Criar novo cliente.
    /// Se o CPF/CNPJ pertencer a um cliente inativo, ele será reativado com os novos dados.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Atendente")]
    [ProducesResponseType(typeof(ClienteDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarClienteDto dto)
    {
        try
        {
            var criado = await _service.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Atualizar dados do cliente (nome, e-mail, telefone, endereço — documento não é alterável)</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Atendente")]
    [ProducesResponseType(typeof(ClienteDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarClienteDto dto)
    {
        try { return Ok(await _service.AtualizarAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Desativar cliente (soft delete)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Desativar(Guid id)
    {
        try
        {
            await _service.DesativarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
