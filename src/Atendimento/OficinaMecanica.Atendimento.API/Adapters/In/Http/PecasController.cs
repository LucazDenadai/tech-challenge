using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.Peca;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("pecas")]
[Authorize]
public class PecasController : ControllerBase
{
    private readonly GerenciarPecaUseCase _useCase;

    public PecasController(GerenciarPecaUseCase useCase) => _useCase = useCase;

    /// <summary>Lista peças ativas. Aceita ?nome= para filtrar por nome.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? nome, CancellationToken ct)
    {
        var resultado = nome is not null
            ? await _useCase.BuscarPorNomeAsync(nome, ct)
            : await _useCase.ObterTodosAsync(ct);
        return Ok(resultado);
    }

    /// <summary>Obtém peça por ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var peca = await _useCase.ObterPorIdAsync(id, ct);
        return peca is null ? NotFound() : Ok(peca);
    }

    /// <summary>Cria nova peça no catálogo.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarPecaRequest request, CancellationToken ct)
    {
        var peca = await _useCase.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = peca.Id }, peca);
    }

    /// <summary>Atualiza peça.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPecaRequest request, CancellationToken ct)
    {
        var peca = await _useCase.AtualizarAsync(id, request, ct);
        return Ok(peca);
    }

    /// <summary>Desativa peça (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        await _useCase.DesativarAsync(id, ct);
        return NoContent();
    }
}
