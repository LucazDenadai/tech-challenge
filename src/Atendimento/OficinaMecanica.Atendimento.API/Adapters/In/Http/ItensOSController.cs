using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("ordens-servico")]
[Authorize]
public class ItensOSController : ControllerBase
{
    private readonly AdicionarItemOSUseCase _adicionarItemUseCase;
    private readonly CancelarItemOSUseCase _cancelarItemUseCase;

    public ItensOSController(
        AdicionarItemOSUseCase adicionarItemUseCase,
        CancelarItemOSUseCase cancelarItemUseCase)
    {
        _adicionarItemUseCase = adicionarItemUseCase;
        _cancelarItemUseCase = cancelarItemUseCase;
    }

    /// <summary>Adiciona serviço à OS (só em EmDiagnostico ou EmExecucao).</summary>
    [HttpPost("{id:guid}/servicos")]
    [Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(typeof(OrdemServicoDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AdicionarServico(Guid id, [FromBody] AdicionarServicoOSRequest request, CancellationToken ct)
    {
        var os = await _adicionarItemUseCase.AdicionarServicoAsync(id, request.ServicoId, ct);
        return Ok(os);
    }

    /// <summary>Adiciona peça à OS (só em EmDiagnostico ou EmExecucao).</summary>
    [HttpPost("{id:guid}/pecas")]
    [Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(typeof(OrdemServicoDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AdicionarPeca(Guid id, [FromBody] AdicionarPecaOSRequest request, CancellationToken ct)
    {
        var os = await _adicionarItemUseCase.AdicionarPecaAsync(id, request.PecaId, request.Quantidade, ct);
        return Ok(os);
    }

    /// <summary>Cancela item da OS (só em EmDiagnostico).</summary>
    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    [Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(typeof(OrdemServicoDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelarItem(Guid id, Guid itemId, CancellationToken ct)
    {
        var os = await _cancelarItemUseCase.ExecutarAsync(id, itemId, ct);
        return Ok(os);
    }
}
