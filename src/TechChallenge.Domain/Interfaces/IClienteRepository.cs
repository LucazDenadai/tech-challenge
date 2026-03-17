using TechChallenge.Domain.Entities;

namespace TechChallenge.Domain.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> ObterPorCpfAsync(string cpf);
    Task<bool> CpfExisteAsync(string cpf, Guid? excluirId = null);
}
