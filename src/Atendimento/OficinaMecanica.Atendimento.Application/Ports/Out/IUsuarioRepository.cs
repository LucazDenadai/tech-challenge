using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Application.Ports.Out;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);
}
