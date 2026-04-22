using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Application.DTOs.Veiculo;
using TechChallenge.Application.Interfaces;

namespace TechChallenge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _service;

    public VeiculosController(IVeiculoService service) => _service = service;

    /// <summary>Listar todos os veículos</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VeiculoDto>), 200)]
    public async Task<IActionResult> ObterTodos() => Ok(await _service.ObterTodosAsync());

    /// <summary>Obter veículo por ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _service.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Listar veículos por ID do cliente</summary>
    [HttpGet("cliente/{clienteId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<VeiculoDto>), 200)]
    public async Task<IActionResult> ObterPorCliente(Guid clienteId)
        => Ok(await _service.ObterPorClienteAsync(clienteId));

    /// <summary>Listar veículos por documento do cliente (CPF ou CNPJ)</summary>
    [HttpGet("cliente/documento/{documento}")]
    [ProducesResponseType(typeof(IEnumerable<VeiculoDto>), 200)]
    public async Task<IActionResult> ObterPorDocumentoCliente(string documento)
        => Ok(await _service.ObterPorDocumentoClienteAsync(documento));

    /// <summary>
    /// Criar novo veículo informando o documento do cliente (CPF/CNPJ) como proprietário.
    /// Placa aceita formato antigo (ABC1234) ou Mercosul (ABC1D23).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VeiculoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarVeiculoDto dto)
    {
        try
        {
            var criado = await _service.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }
        catch (KeyNotFoundException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Atualizar veículo (marca, modelo, ano, cor)</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarVeiculoDto dto)
    {
        try { return Ok(await _service.AtualizarAsync(id, dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Remover veículo (não permitido se existirem OS vinculadas)</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Remover(Guid id)
    {
        try { await _service.RemoverAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
