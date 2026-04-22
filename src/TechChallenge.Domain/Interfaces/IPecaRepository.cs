using TechChallenge.Domain.Entities;

namespace TechChallenge.Domain.Interfaces;

public interface IPecaRepository : IRepository<Peca>
{
    Task<IEnumerable<Peca>> ObterAtivosAsync();
    Task<IEnumerable<Peca>> BuscarPorNomeAsync(string nome);
}
