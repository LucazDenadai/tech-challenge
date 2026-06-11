using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Estoque.Application.Ports.Out;

namespace OficinaMecanica.Estoque.API.Controllers;

[ApiController]
[Route("estoque/falhas")]
public class FalhasController(IFalhaRepository falhaRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var falhas = await falhaRepository.ObterTodosAsync(ct);
        return Ok(falhas);
    }

    [HttpGet("{osId:guid}")]
    public async Task<IActionResult> ListarPorOs(Guid osId, CancellationToken ct)
    {
        var falhas = await falhaRepository.ObterPorOrdemServicoIdAsync(osId, ct);
        return Ok(falhas);
    }
}
