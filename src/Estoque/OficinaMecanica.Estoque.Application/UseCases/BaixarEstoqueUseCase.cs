using OficinaMecanica.Estoque.Application.Exceptions;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Domain.Entities;
using OficinaMecanica.Estoque.Domain.Enums;

namespace OficinaMecanica.Estoque.Application.UseCases;

public record ItemBaixa(Guid PecaId, int Quantidade);

public class BaixarEstoqueUseCase(IPecaRepository pecaRepository, IMovimentacaoRepository movimentacaoRepository)
{
    public async Task ExecutarAsync(Guid osId, IEnumerable<ItemBaixa> itens, CancellationToken ct = default)
    {
        if (await movimentacaoRepository.ExisteMovimentacaoPorOsIdAsync(osId, ct))
            return;

        foreach (var item in itens)
        {
            var peca = await pecaRepository.ObterPorIdAsync(item.PecaId, ct)
                ?? throw new NotFoundException(nameof(Peca), item.PecaId);

            peca.SubtrairEstoque(item.Quantidade);

            var movimentacao = new MovimentacaoEstoque(peca.Id, TipoMovimentacao.Saida, item.Quantidade, "Baixa por OS finalizada", osId);

            await pecaRepository.AtualizarAsync(peca, ct);
            await movimentacaoRepository.AdicionarAsync(movimentacao, ct);
        }

        await movimentacaoRepository.SalvarAsync(ct);
    }
}
