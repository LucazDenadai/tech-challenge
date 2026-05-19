using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.Veiculo;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("veiculos")]
[Authorize]
public class VeiculosController : ControllerBase
{
    private readonly GerenciarVeiculoUseCase _useCase;

    public VeiculosController(GerenciarVeiculoUseCase useCase) => _useCase = useCase;

    /// <summary>Cadastra um novo veículo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar([FromBody] CriarVeiculoRequest request, CancellationToken ct)
    {
        var id = await _useCase.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
    }

    /// <summary>Obtém um veículo pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId(Guid id) => Ok(new { id });

    /// <summary>Atualiza dados de um veículo.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarVeiculoRequest request, CancellationToken ct)
    {
        await _useCase.AtualizarAsync(request with { Id = id }, ct);
        return NoContent();
    }
}
