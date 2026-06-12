using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("ordens-servico")]
[Authorize]
public class OrdensServicoController : ControllerBase
{
    private readonly AbrirOrdemServicoUseCase _abrirUseCase;
    private readonly ListarOrdensServicoUseCase _listarUseCase;
    private readonly ObterOrdemServicoUseCase _obterUseCase;

    public OrdensServicoController(
        AbrirOrdemServicoUseCase abrirUseCase,
        ListarOrdensServicoUseCase listarUseCase,
        ObterOrdemServicoUseCase obterUseCase)
    {
        _abrirUseCase = abrirUseCase;
        _listarUseCase = listarUseCase;
        _obterUseCase = obterUseCase;
    }

    /// <summary>Lista todas as ordens de serviço.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ListarOrdensServicoItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var resultado = await _listarUseCase.ExecutarAsync(ct);
        return Ok(resultado);
    }

    /// <summary>Abre uma nova ordem de serviço.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AbrirOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Abrir([FromBody] AbrirOrdemServicoRequest request, CancellationToken ct)
    {
        var resultado = await _abrirUseCase.ExecutarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>Obtém OS completa por ID (itens + histórico).</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Atendente,Mecanico")]
    [ProducesResponseType(typeof(OrdemServicoDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var os = await _obterUseCase.ExecutarAsync(id, ct);
        return os is null ? NotFound() : Ok(os);
    }
}
