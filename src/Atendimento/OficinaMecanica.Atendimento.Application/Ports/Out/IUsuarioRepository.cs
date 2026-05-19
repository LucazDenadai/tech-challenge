namespace OficinaMecanica.Atendimento.Application.Ports.Out;

// Usuario pertence ao contexto de autenticação; aqui o Application só precisa validar existência.
public interface IUsuarioRepository
{
    Task<bool> ExisteAsync(Guid usuarioId, CancellationToken ct = default);
    Task<string?> ObterEmailAsync(Guid usuarioId, CancellationToken ct = default);
}
