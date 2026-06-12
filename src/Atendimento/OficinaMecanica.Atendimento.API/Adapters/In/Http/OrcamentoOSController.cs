using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("ordens-servico")]
[Authorize]
public class OrcamentoOSController : ControllerBase
{
    private readonly AprovarOrcamentoUseCase _aprovarOrcamentoUseCase;

    public OrcamentoOSController(AprovarOrcamentoUseCase aprovarOrcamentoUseCase)
    {
        _aprovarOrcamentoUseCase = aprovarOrcamentoUseCase;
    }

    /// <summary>Aprova ou recusa o orçamento de uma ordem de serviço.</summary>
    [HttpPut("{id:guid}/orcamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AprovarOrcamento(Guid id, [FromBody] AprovarOrcamentoRequest request, CancellationToken ct)
    {
        await _aprovarOrcamentoUseCase.ExecutarAsync(id, request.Aprovado, ct);
        return NoContent();
    }
}

public record AprovarOrcamentoRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public required bool Aprovado { get; init; }
}
