using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class CancelarItemOSUseCase(IOrdemServicoRepository repository)
{
    public async Task<OrdemServicoDetalheResponse> ExecutarAsync(Guid osId, Guid itemId, CancellationToken ct = default)
    {
        var os = await repository.ObterComDetalhesAsync(osId, ct)
            ?? throw new NotFoundException("OrdemServico", osId);

        try { os.RemoverItemServico(itemId); }
        catch (KeyNotFoundException) { os.RemoverItemPeca(itemId); }

        await repository.AtualizarAsync(os, ct);
        await repository.SalvarAsync(ct);
        return ObterOrdemServicoUseCase.ToDetalhe(os);
    }
}
