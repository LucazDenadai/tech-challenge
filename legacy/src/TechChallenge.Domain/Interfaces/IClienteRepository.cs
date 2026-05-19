using TechChallenge.Domain.Entities;

namespace TechChallenge.Domain.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> ObterPorDocumentoAsync(string documento);
    Task<bool> DocumentoExisteAsync(string documento, Guid? excluirId = null);
    Task<IEnumerable<Cliente>> BuscarAsync(string termo);
    Task<IEnumerable<(Cliente Cliente, int TotalOrdens)>> ObterTodosComDetalhesAsync();
    Task<IEnumerable<(Cliente Cliente, int TotalOrdens)>> BuscarComDetalhesAsync(string termo);
}
