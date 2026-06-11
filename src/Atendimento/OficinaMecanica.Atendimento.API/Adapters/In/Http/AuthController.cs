using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Atendimento.Application.UseCases.Auth;

namespace OficinaMecanica.Atendimento.API.Adapters.In.Http;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthUseCase _useCase;

    public AuthController(AuthUseCase useCase) => _useCase = useCase;

    /// <summary>Realiza login e retorna JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var token = await _useCase.LoginAsync(request, ct);
        return Ok(new { token });
    }
}
