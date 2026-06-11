using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.Usuario;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("usuarios")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly GerenciarUsuarioUseCase _useCase;

    public UsuariosController(GerenciarUsuarioUseCase useCase) => _useCase = useCase;

    /// <summary>Lista usuários ativos. Aceita ?email= para filtrar.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? email, CancellationToken ct)
    {
        var resultado = email is not null
            ? await _useCase.BuscarPorEmailAsync(email, ct)
            : await _useCase.ObterTodosAsync(ct);
        return Ok(resultado);
    }

    /// <summary>Obtém usuário por ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var usuario = await _useCase.ObterPorIdAsync(id, ct);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    /// <summary>Cria novo usuário.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioRequest request, CancellationToken ct)
    {
        var usuario = await _useCase.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
    }

    /// <summary>Atualiza nome, email, perfil e senha de um usuário.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarUsuarioRequest request, CancellationToken ct)
    {
        var usuario = await _useCase.AtualizarAsync(id, request, ct);
        return Ok(usuario);
    }

    /// <summary>Desativa um usuário (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        await _useCase.DesativarAsync(id, ct);
        return NoContent();
    }
}
