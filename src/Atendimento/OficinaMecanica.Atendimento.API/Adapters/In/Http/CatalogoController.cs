using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Application.UseCases.Catalogo;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Authorize]
public class CatalogoController : ControllerBase
{
    private readonly GerenciarCatalogoUseCase _useCase;
    private readonly IPecaRepository _pecaRepository;

    public CatalogoController(GerenciarCatalogoUseCase useCase, IPecaRepository pecaRepository)
    {
        _useCase = useCase;
        _pecaRepository = pecaRepository;
    }

    // ── Serviços ──────────────────────────────────────────────────────────────

    /// <summary>Cria um novo serviço no catálogo.</summary>
    [HttpPost("catalogo/servicos")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CriarServico([FromBody] CriarServicoRequest request, CancellationToken ct)
    {
        var id = await _useCase.CriarServicoAsync(request, ct);
        return CreatedAtAction(nameof(CriarServico), new { id }, new { id });
    }

    /// <summary>Atualiza um serviço do catálogo.</summary>
    [HttpPut("catalogo/servicos/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AtualizarServico(Guid id, [FromBody] AtualizarServicoRequest request, CancellationToken ct)
    {
        await _useCase.AtualizarServicoAsync(request with { Id = id }, ct);
        return NoContent();
    }

    /// <summary>Desativa um serviço do catálogo.</summary>
    [HttpDelete("catalogo/servicos/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesativarServico(Guid id, CancellationToken ct)
    {
        await _useCase.DesativarServicoAsync(id, ct);
        return NoContent();
    }

    // ── Peças (gerenciadas pelo microsserviço Estoque) ─────────────────────────

    /// <summary>Verifica se uma peça existe no estoque.</summary>
    [HttpGet("catalogo/pecas/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerificarPeca(Guid id, CancellationToken ct)
    {
        var existe = await _pecaRepository.ExisteAsync(id, ct);
        if (!existe)
            return NotFound(new ProblemDetails { Status = 404, Title = "Peça não encontrada", Detail = $"Peça {id} não encontrada." });

        var nome = await _pecaRepository.ObterNomeAsync(id, ct);
        return Ok(new { id, nome });
    }
}
