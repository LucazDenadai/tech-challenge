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
    private readonly ObterOrdemServicoUseCase _obterUseCase;
    private readonly AcompanharOSUseCase _acompanharUseCase;
    private readonly AdicionarItemOSUseCase _adicionarItemUseCase;
    private readonly CancelarItemOSUseCase _cancelarItemUseCase;
    private readonly ObterTempoExecucaoUseCase _tempoUseCase;

    public OrdensServicoController(
        AbrirOrdemServicoUseCase abrirUseCase,
        ConsultarStatusOSUseCase consultarStatusUseCase,
        AprovarOrcamentoUseCase aprovarOrcamentoUseCase,
        ListarOrdensServicoUseCase listarUseCase,
        AtualizarStatusOSUseCase atualizarStatusUseCase,
        ObterOrdemServicoUseCase obterUseCase,
        AcompanharOSUseCase acompanharUseCase,
        AdicionarItemOSUseCase adicionarItemUseCase,
        CancelarItemOSUseCase cancelarItemUseCase,
        ObterTempoExecucaoUseCase tempoUseCase)
    {
        _abrirUseCase = abrirUseCase;
        _consultarStatusUseCase = consultarStatusUseCase;
        _aprovarOrcamentoUseCase = aprovarOrcamentoUseCase;
        _listarUseCase = listarUseCase;
        _atualizarStatusUseCase = atualizarStatusUseCase;
        _obterUseCase = obterUseCase;
        _acompanharUseCase = acompanharUseCase;
        _adicionarItemUseCase = adicionarItemUseCase;
        _cancelarItemUseCase = cancelarItemUseCase;
        _tempoUseCase = tempoUseCase;
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

public record AprovarOrcamentoRequest(bool Aprovado);
public record AtualizarStatusOSRequest(StatusOrdemServico NovoStatus);
