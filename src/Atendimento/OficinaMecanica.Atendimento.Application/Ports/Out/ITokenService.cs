namespace OficinaMecanica.Atendimento.Application.Ports.Out;

// TODO: implementado no CARD-05 (Infrastructure — JWT via BCrypt + System.IdentityModel)
public interface ITokenService
{
    string GerarToken(Guid usuarioId, string email, string perfil);
    bool VerificarSenha(string senhaPlana, string hashArmazenado);
}
