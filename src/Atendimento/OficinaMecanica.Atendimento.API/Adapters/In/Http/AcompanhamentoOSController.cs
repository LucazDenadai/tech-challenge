using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("atendimento/ordens-servico")]
[Authorize]
public class AcompanhamentoOSController : ControllerBase
{
    private readonly AcompanharOSUseCase _acompanharUseCase;

    public AcompanhamentoOSController(AcompanharOSUseCase acompanharUseCase)
    {
        _acompanharUseCase = acompanharUseCase;
    }

    /// <summary>Acompanhamento público da OS pelo número — sem autenticação.</summary>
    [HttpGet("acompanhar/{numero}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AcompanhamentoOSResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Acompanhar(string numero, CancellationToken ct)
    {
        var os = await _acompanharUseCase.ExecutarAsync(numero, ct);
        return os is null ? NotFound() : Ok(os);
    }
}
