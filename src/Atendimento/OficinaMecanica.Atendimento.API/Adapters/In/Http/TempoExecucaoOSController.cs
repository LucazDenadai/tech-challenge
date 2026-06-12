using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("ordens-servico")]
[Authorize]
public class TempoExecucaoOSController : ControllerBase
{
    private readonly ObterTempoExecucaoUseCase _tempoUseCase;

    public TempoExecucaoOSController(ObterTempoExecucaoUseCase tempoUseCase)
    {
        _tempoUseCase = tempoUseCase;
    }

    /// <summary>Tempo médio global de execução das OS finalizadas (em horas).</summary>
    [HttpGet("tempo-medio")]
    [Authorize(Roles = "Admin,Atendente")]
    [ProducesResponseType(typeof(TempoMedioResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTempoMedio(CancellationToken ct)
        => Ok(await _tempoUseCase.ObterTempoMedioAsync(ct));

    /// <summary>Tempo de execução individual de uma OS pelo número.</summary>
    [HttpGet("{numero}/tempo")]
    [Authorize(Roles = "Admin,Atendente,Mecanico")]
    [ProducesResponseType(typeof(TempoIndividualResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterTempoIndividual(string numero, CancellationToken ct)
    {
        var resultado = await _tempoUseCase.ObterTempoIndividualAsync(numero, ct);
        return resultado is null ? NotFound() : Ok(resultado);
    }
}
