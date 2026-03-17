using Microsoft.AspNetCore.Mvc;
using TechChallenge.Application.DTOs.Auth;
using TechChallenge.Application.Interfaces;

namespace TechChallenge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Autenticar usuário e obter token JWT</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result is null) return Unauthorized(new { message = "Credenciais inválidas." });
        return Ok(result);
    }
}
