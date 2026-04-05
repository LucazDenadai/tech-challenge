using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Application.DTOs.OrdemServico;
using TechChallenge.Application.Interfaces;
using TechChallenge.Domain.Enums;

namespace TechChallenge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrdensServicoController : ControllerBase
{
    private readonly IOrdemServicoService _service;

    public OrdensServicoController(IOrdemServicoService service) => _service = service;

    /// <summary>Listar todas as ordens de serviço</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoDto>), 200)]
    public async Task<IActionResult> ObterTodos() => Ok(await _service.ObterTodosAsync());

    /// <summary>Obter ordem de serviço por ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _service.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Listar ordens por cliente</summary>
    [HttpGet("cliente/{clienteId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoDto>), 200)]
    public async Task<IActionResult> ObterPorCliente(Guid clienteId)
        => Ok(await _service.ObterPorClienteAsync(clienteId));

    /// <summary>Listar ordens por status</summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoDto>), 200)]
    public async Task<IActionResult> ObterPorStatus(StatusOrdemServico status)
        => Ok(await _service.ObterPorStatusAsync(status));

    /// <summary>Criar nova ordem de serviço</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrdemServicoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarOrdemServicoDto dto)
    {
        try
        {
            var criado = await _service.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }
        catch (KeyNotFoundException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Avançar status da ordem de serviço</summary>
    [HttpPatch("{id:guid}/avancar-status")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AvancarStatus(Guid id)
    {
        return Ok(await _service.AvancarStatusAsync(id));
    }

    /// <summary>Adicionar serviço à ordem</summary>
    [HttpPost("{id:guid}/servicos")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AdicionarServico(Guid id, [FromBody] AdicionarItemServicoDto dto)
    {
        return Ok(await _service.AdicionarItemServicoAsync(id, dto));
    }

    /// <summary>Adicionar peça à ordem</summary>
    [HttpPost("{id:guid}/pecas")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AdicionarPeca(Guid id, [FromBody] AdicionarItemPecaDto dto)
    {
        return Ok(await _service.AdicionarItemPecaAsync(id, dto));
    }
}
