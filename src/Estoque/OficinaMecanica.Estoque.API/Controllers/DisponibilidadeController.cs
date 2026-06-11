using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Estoque.Application.UseCases;

namespace OficinaMecanica.Estoque.API.Controllers;

[ApiController]
[Route("estoque")]
public class DisponibilidadeController(ConsultarDisponibilidadeUseCase useCase) : ControllerBase
{
    [HttpPost("disponibilidade")]
    public async Task<IActionResult> ConsultarDisponibilidade(
        [FromBody] DisponibilidadeRequest request, CancellationToken ct)
    {
        var disponivel = await useCase.ExecutarAsync(request.Itens, ct);
        return Ok(new DisponibilidadeResponse(disponivel));
    }
}

public record DisponibilidadeRequest(List<ItemDisponibilidade> Itens);
public record DisponibilidadeResponse(bool Disponivel);
