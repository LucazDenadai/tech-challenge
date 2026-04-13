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

    /// <summary>
    /// Listar ordens de serviço com filtros opcionais.
    /// clienteId: filtra pelo cliente. status: Recebida | EmDiagnostico | AguardandoAprovacao | EmExecucao | Finalizada | Entregue
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? clienteId,
        [FromQuery] StatusOrdemServico? status)
        => Ok(await _service.FiltrarAsync(clienteId, status));

    /// <summary>Obter ordem de serviço por ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _service.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

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

    /// <summary>
    /// Alterar status da OS. Para avançar informe o próximo status na sequência.
    /// Para cancelar informe Cancelada — permitido a qualquer momento.
    /// Sequência: Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] AlterarStatusDto dto)
        => Ok(await _service.AlterarStatusAsync(id, dto));

    /// <summary>Adicionar serviço ou peça à ordem (só em EmDiagnostico ou EmExecucao)</summary>
    [HttpPost("{id:guid}/itens")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AdicionarItem(Guid id, [FromBody] AdicionarItemDto dto)
        => Ok(await _service.AdicionarItemAsync(id, dto));

    /// <summary>Tempo médio de execução das ordens finalizadas (em horas)</summary>
    [HttpGet("tempo-medio")]
    [ProducesResponseType(typeof(TempoMedioExecucaoDto), 200)]
    public async Task<IActionResult> ObterTempoMedio()
        => Ok(await _service.ObterTempoMedioExecucaoAsync());

    /// <summary>Cancelar item da ordem (só em EmDiagnostico)</summary>
    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelarItem(Guid id, Guid itemId)
        => Ok(await _service.CancelarItemAsync(id, itemId));
}
