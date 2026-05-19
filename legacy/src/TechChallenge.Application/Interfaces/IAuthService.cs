using TechChallenge.Application.DTOs.Auth;

namespace TechChallenge.Application.Interfaces;

public interface IAuthService
{
    Task<TokenResponseDto?> LoginAsync(LoginDto loginDto);
}
