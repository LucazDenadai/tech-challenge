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

    /// <summary>Lista clientes ativos. Aceita ?busca= para filtrar por nome, CPF/CNPJ ou e-mail.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Atendente")]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? busca, CancellationToken ct)
    {
        var resultado = busca is not null
            ? await _useCase.BuscarAsync(busca, ct)
            : await _useCase.ObterTodosAsync(ct);
        return Ok(resultado);
    }

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
    [Authorize(Roles = "Admin,Atendente")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var cliente = await _useCase.ObterPorIdAsync(id, ct);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    /// <summary>Atualiza nome, e-mail, telefone e endereço do cliente. Documento não é alterável.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Atendente")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarClienteRequest request, CancellationToken ct)
    {
        var cliente = await _useCase.AtualizarAsync(request with { Id = id }, ct);
        return Ok(cliente);
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
