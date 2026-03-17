using TechChallenge.Domain.Entities;

namespace TechChallenge.Domain.Interfaces;

public interface IServicoRepository : IRepository<Servico>
{
    Task<IEnumerable<Servico>> ObterAtivosAsync();
}
