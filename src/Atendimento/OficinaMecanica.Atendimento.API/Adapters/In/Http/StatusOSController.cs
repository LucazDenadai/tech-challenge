using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;
using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("atendimento/ordens-servico")]
[Authorize]
public class StatusOSController : ControllerBase
{
    private readonly ConsultarStatusOSUseCase _consultarStatusUseCase;
    private readonly AtualizarStatusOSUseCase _atualizarStatusUseCase;

    public StatusOSController(
        ConsultarStatusOSUseCase consultarStatusUseCase,
        AtualizarStatusOSUseCase atualizarStatusUseCase)
    {
        _consultarStatusUseCase = consultarStatusUseCase;
        _atualizarStatusUseCase = atualizarStatusUseCase;
    }

    /// <summary>Consulta o status de uma ordem de serviço.</summary>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(ConsultarStatusOSResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarStatus(Guid id, CancellationToken ct)
    {
        var resultado = await _consultarStatusUseCase.ExecutarAsync(id, ct);
        return Ok(resultado);
    }

    /// <summary>Atualiza o status de uma ordem de serviço.</summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] AtualizarStatusOSRequest request, CancellationToken ct)
    {
        await _atualizarStatusUseCase.ExecutarAsync(id, request.NovoStatus, ct);
        return NoContent();
    }
}

public record AtualizarStatusOSRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public required StatusOrdemServico NovoStatus { get; init; }
}
