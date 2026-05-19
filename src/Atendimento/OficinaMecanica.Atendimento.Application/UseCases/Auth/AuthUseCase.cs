using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Application.UseCases.Auth;

public class AuthUseCase(IUsuarioRepository usuarioRepository, ITokenService tokenService)
{
    public async Task<string> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await usuarioRepository.ObterPorEmailAsync(request.Email, ct)
            ?? throw new NotFoundException("Usuario", request.Email);

        if (!tokenService.VerificarSenha(request.Senha, usuario.SenhaHash))
            throw new InvalidOperationException("Senha incorreta.");

        return tokenService.GerarToken(usuario.Id, usuario.Email, usuario.Perfil.ToString());
    }
}
