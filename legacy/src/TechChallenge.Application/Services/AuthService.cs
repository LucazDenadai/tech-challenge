using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TechChallenge.Application.DTOs.Auth;
using TechChallenge.Application.Interfaces;
using TechChallenge.Domain.Interfaces;

namespace TechChallenge.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repo;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IUsuarioRepository repo, IConfiguration config,
        ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _config = config;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var usuario = await _repo.ObterPorEmailAsync(loginDto.Email);

        if (usuario is null || !usuario.Ativo)
        {
            _logger.LogWarning("[AUDIT] Login falhou — usuario nao encontrado ou inativo | Email={Email} | IP={IP}", loginDto.Email, ip);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("[AUDIT] Login falhou — senha incorreta | Email={Email} | IP={IP}", loginDto.Email, ip);
            return null;
        }

        var jwtKey = (_config["Jwt:Key"] is { Length: > 0 } k ? k : null)
            ?? Environment.GetEnvironmentVariable("JWT_KEY")
            ?? "";
        var jwtIssuer = _config["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "";
        var jwtAudience = _config["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "";
        var expiracaoMinutos = int.Parse(_config["Jwt:ExpiracaoMinutos"] ?? Environment.GetEnvironmentVariable("JWT_EXPIRACAO_MINUTOS") ?? "60");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiracao = DateTime.UtcNow.AddMinutes(expiracaoMinutos);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiracao,
            signingCredentials: creds);

        var resultado = new TokenResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracao = expiracao
        };

        _logger.LogInformation("[AUDIT] Login bem-sucedido | Usuario={Usuario} | Email={Email} | Perfil={Perfil} | IP={IP}",
            usuario.Nome, usuario.Email, usuario.Perfil, ip);

        return resultado;
    }
}
