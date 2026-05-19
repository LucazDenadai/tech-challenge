using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;
using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("ordens-servico")]
[Authorize]
public class OrdensServicoController : ControllerBase
{
    private readonly AbrirOrdemServicoUseCase _abrirUseCase;
    private readonly ConsultarStatusOSUseCase _consultarStatusUseCase;
    private readonly AprovarOrcamentoUseCase _aprovarOrcamentoUseCase;
    private readonly ListarOrdensServicoUseCase _listarUseCase;
    private readonly AtualizarStatusOSUseCase _atualizarStatusUseCase;

    public OrdensServicoController(
        AbrirOrdemServicoUseCase abrirUseCase,
        ConsultarStatusOSUseCase consultarStatusUseCase,
        AprovarOrcamentoUseCase aprovarOrcamentoUseCase,
        ListarOrdensServicoUseCase listarUseCase,
        AtualizarStatusOSUseCase atualizarStatusUseCase)
    {
        _abrirUseCase = abrirUseCase;
        _consultarStatusUseCase = consultarStatusUseCase;
        _aprovarOrcamentoUseCase = aprovarOrcamentoUseCase;
        _listarUseCase = listarUseCase;
        _atualizarStatusUseCase = atualizarStatusUseCase;
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
        return CreatedAtAction(nameof(ConsultarStatus), new { id = resultado.Id }, resultado);
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

public record AprovarOrcamentoRequest(bool Aprovado);
public record AtualizarStatusOSRequest(StatusOrdemServico NovoStatus);
