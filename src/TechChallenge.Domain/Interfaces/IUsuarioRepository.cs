using TechChallenge.Domain.Entities;

namespace TechChallenge.Domain.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<bool> EmailExisteAsync(string email, Guid? excluirId = null);
    Task<IEnumerable<Usuario>> BuscarPorEmailAsync(string email);
}
