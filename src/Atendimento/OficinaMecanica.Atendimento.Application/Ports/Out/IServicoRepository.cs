using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Application.Ports.Out;

public interface IServicoRepository : IRepository<Servico>
{
    Task<IEnumerable<Servico>> ObterAtivosAsync(CancellationToken ct = default);
}
