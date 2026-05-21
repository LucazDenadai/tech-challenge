using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class AdicionarItemOSUseCase(
    IOrdemServicoRepository osRepository,
    IServicoRepository servicoRepository,
    IPecaRepository pecaRepository)
{
    public async Task<OrdemServicoDetalheResponse> AdicionarServicoAsync(Guid osId, Guid servicoId, CancellationToken ct = default)
    {
        var os = await osRepository.ObterComDetalhesAsync(osId, ct)
            ?? throw new NotFoundException("OrdemServico", osId);

        var servico = await servicoRepository.ObterPorIdAsync(servicoId, ct)
            ?? throw new NotFoundException("Servico", servicoId);

        os.AdicionarItemServico(new ItemServico(os.Id, servico.Id, 1, servico.Preco));
        await osRepository.AtualizarAsync(os, ct);
        await osRepository.SalvarAsync(ct);
        return ObterOrdemServicoUseCase.ToDetalhe(os);
    }

    public async Task<OrdemServicoDetalheResponse> AdicionarPecaAsync(Guid osId, Guid pecaId, int quantidade, CancellationToken ct = default)
    {
        var os = await osRepository.ObterComDetalhesAsync(osId, ct)
            ?? throw new NotFoundException("OrdemServico", osId);

        var peca = await pecaRepository.ObterPorIdAsync(pecaId, ct)
            ?? throw new NotFoundException("Peca", pecaId);

        os.AdicionarItemPeca(new ItemPeca(os.Id, peca.Id, quantidade, peca.Preco));
        await osRepository.AtualizarAsync(os, ct);
        await osRepository.SalvarAsync(ct);
        return ObterOrdemServicoUseCase.ToDetalhe(os);
    }
}
