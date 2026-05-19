using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.Cliente;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly GerenciarClienteUseCase _useCase;

    public ClientesController(GerenciarClienteUseCase useCase) => _useCase = useCase;

    /// <summary>Cria um novo cliente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar([FromBody] CriarClienteRequest request, CancellationToken ct)
    {
        var id = await _useCase.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
    }

    /// <summary>Obtém um cliente pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId(Guid id) => Ok(new { id });

    /// <summary>Atualiza dados de um cliente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarClienteRequest request, CancellationToken ct)
    {
        await _useCase.AtualizarAsync(request with { Id = id }, ct);
        return NoContent();
    }

    /// <summary>Desativa um cliente.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        await _useCase.DesativarAsync(id, ct);
        return NoContent();
    }
}
