namespace TechChallenge.Application.DTOs.Auth;

public class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracao { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
}
