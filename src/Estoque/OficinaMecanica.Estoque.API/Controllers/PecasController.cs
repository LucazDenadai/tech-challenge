using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Estoque.Application.UseCases;

namespace OficinaMecanica.Estoque.API.Controllers;

[ApiController]
[Route("estoque/pecas")]
public class PecasController(GerenciarPecaUseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObterTodos(CancellationToken ct)
    {
        var pecas = await useCase.ObterTodosAsync(ct);
        return Ok(pecas);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var peca = await useCase.ObterPorIdAsync(id, ct);
        return peca is null ? NotFound() : Ok(peca);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPecaRequest request, CancellationToken ct)
    {
        var peca = await useCase.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = peca.Id }, peca);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPecaRequest request, CancellationToken ct)
    {
        if (id != request.Id) return BadRequest("Id da rota difere do corpo da requisição.");
        await useCase.AtualizarAsync(request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await useCase.RemoverAsync(id, ct);
        return NoContent();
    }
}
