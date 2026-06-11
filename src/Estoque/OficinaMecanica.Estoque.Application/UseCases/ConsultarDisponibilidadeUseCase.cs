using OficinaMecanica.Estoque.Application.Ports.Out;

namespace OficinaMecanica.Estoque.Application.UseCases;

public record ItemDisponibilidade(Guid PecaId, int QuantidadeSolicitada);

public class ConsultarDisponibilidadeUseCase(IPecaRepository pecaRepository)
{
    public async Task<bool> ExecutarAsync(IEnumerable<ItemDisponibilidade> itens, CancellationToken ct = default)
    {
        var listaItens = itens.ToList();
        var ids = listaItens.Select(i => i.PecaId);
        var pecas = (await pecaRepository.ObterPorIdsAsync(ids, ct)).ToDictionary(p => p.Id);

        return listaItens.All(item =>
            pecas.TryGetValue(item.PecaId, out var peca) &&
            peca.QuantidadeEstoque >= item.QuantidadeSolicitada);
    }
}
