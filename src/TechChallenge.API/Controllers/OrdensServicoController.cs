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
    /// Listar ordens de serviço. Use ?busca= para filtrar por número da OS, documento do cliente ou placa do veículo.
    /// Use ?status= para filtrar por status: Recebida | EmDiagnostico | AguardandoAprovacao | EmExecucao | Finalizada | Entregue | Cancelada
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] StatusOrdemServico? status)
        => Ok(await _service.FiltrarAsync(busca, status));

    /// <summary>Obter ordem de serviço por ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _service.ObterPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>
    /// Acompanhar OS pelo número — acesso público, sem autenticação.
    /// O cliente informa o número da OS (ex: OS-2026-0001) e recebe o status atual, serviços, peças e histórico.
    /// Dados pessoais do cliente não são expostos neste endpoint.
    /// </summary>
    [HttpGet("acompanhar/{numero}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AcompanhamentoOsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Acompanhar(string numero)
    {
        var dto = await _service.AcompanharPorNumeroAsync(numero);
        return dto is null ? NotFound(new { message = $"OS '{numero}' não encontrada." }) : Ok(dto);
    }

    /// <summary>
    /// Criar nova ordem de serviço informando o documento do cliente (CPF/CNPJ) e a placa do veículo.
    /// </summary>
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
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Alterar status da OS informando o número (ex: OS-2026-0001).
    /// Sequência: Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue
    /// Para cancelar informe Cancelada — permitido a qualquer momento.
    /// Status disponíveis: 1=Recebida | 2=EmDiagnostico | 3=AguardandoAprovacao | 4=EmExecucao | 5=Finalizada | 6=Entregue | 7=Cancelada
    /// </summary>
    [HttpPatch("{numero}/status")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AlterarStatus(string numero, [FromBody] AlterarStatusDto dto)
    {
        try { return Ok(await _service.AlterarStatusAsync(numero, dto)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Adicionar serviço à ordem (só em EmDiagnostico ou EmExecucao)</summary>
    [HttpPost("{id:guid}/servicos")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AdicionarServico(Guid id, [FromBody] AdicionarItemServicoDto dto)
    {
        var item = new AdicionarItemDto { Tipo = TipoItem.Servico, ItemId = dto.ServicoId, Quantidade = 1 };
        try { return Ok(await _service.AdicionarItemAsync(id, item)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Adicionar peça à ordem (só em EmDiagnostico ou EmExecucao)</summary>
    [HttpPost("{id:guid}/pecas")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AdicionarPeca(Guid id, [FromBody] AdicionarItemPecaDto dto)
    {
        var item = new AdicionarItemDto { Tipo = TipoItem.Peca, ItemId = dto.PecaId, Quantidade = dto.Quantidade };
        try { return Ok(await _service.AdicionarItemAsync(id, item)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Tempo médio global de execução das ordens finalizadas (em horas)</summary>
    [HttpGet("tempo-medio")]
    [ProducesResponseType(typeof(TempoMedioExecucaoDto), 200)]
    public async Task<IActionResult> ObterTempoMedio()
        => Ok(await _service.ObterTempoMedioExecucaoAsync());

    /// <summary>
    /// Tempo de execução de uma OS específica pelo número (ex: OS-2026-0001).
    /// Para OS finalizada mostra o tempo real; para OS em andamento mostra o tempo decorrido até agora.
    /// Referência: tempo ideal de 1 a 3 dias úteis.
    /// </summary>
    [HttpGet("{numero}/tempo")]
    [ProducesResponseType(typeof(TempoIndividualOsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterTempoIndividual(string numero)
    {
        var dto = await _service.ObterTempoIndividualAsync(numero);
        return dto is null ? NotFound(new { message = $"OS '{numero}' não encontrada." }) : Ok(dto);
    }

    /// <summary>Cancelar item da ordem (só em EmDiagnostico)</summary>
    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    [ProducesResponseType(typeof(OrdemServicoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelarItem(Guid id, Guid itemId)
    {
        try { return Ok(await _service.CancelarItemAsync(id, itemId)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
