using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.Catalogo;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("atendimento/servicos")]
[Authorize]
public class ServicosController : ControllerBase
{
    private readonly GerenciarCatalogoUseCase _useCase;

    public ServicosController(GerenciarCatalogoUseCase useCase) => _useCase = useCase;

    /// <summary>Lista todos os serviços ativos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServicoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await _useCase.ObterServicosAsync(ct));

    /// <summary>Obtém serviço por ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var servico = await _useCase.ObterServicoPorIdAsync(id, ct);
        return servico is null ? NotFound() : Ok(servico);
    }

    /// <summary>Cria novo serviço.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarServicoRequest request, CancellationToken ct)
    {
        var servico = await _useCase.CriarServicoAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = servico.Id }, servico);
    }

    /// <summary>Atualiza serviço.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarServicoRequest request, CancellationToken ct)
    {
        var servico = await _useCase.AtualizarServicoAsync(
            new AtualizarServicoRequest(id, request.Nome, request.Descricao, request.Preco, request.TempoConclusaoMinutos), ct);
        return Ok(servico);
    }

    /// <summary>Desativa serviço (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        await _useCase.DesativarServicoAsync(id, ct);
        return NoContent();
    }
}
