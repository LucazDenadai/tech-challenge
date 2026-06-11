using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using DomainOS = OficinaMecanica.Atendimento.Domain.Entities.OrdemServico;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class AbrirOrdemServicoUseCase(
    IOrdemServicoRepository osRepository,
    IVeiculoRepository veiculoRepository,
    IEstoquePort estoquePort)
{
    public async Task<AbrirOrdemServicoResponse> ExecutarAsync(AbrirOrdemServicoRequest request, CancellationToken ct = default)
    {
        var veiculo = await veiculoRepository.ObterPorIdAsync(request.VeiculoId, ct)
            ?? throw new NotFoundException("Veiculo", request.VeiculoId);

        if (veiculo.ClienteId != request.ClienteId)
            throw new InvalidOperationException("O veículo não pertence ao cliente informado.");

        var pecas = request.Pecas.ToList();
        if (pecas.Count > 0)
        {
            var disponivel = await estoquePort.VerificarDisponibilidadeAsync(pecas, ct);
            if (!disponivel)
                throw new InvalidOperationException("Uma ou mais peças estão indisponíveis no estoque.");
        }

        var numero = await osRepository.GerarNumeroAsync(ct);
        var os = new DomainOS(numero, request.ClienteId, request.VeiculoId, request.Observacoes);

        await osRepository.AdicionarAsync(os, ct);
        await osRepository.SalvarAsync(ct);

        return new AbrirOrdemServicoResponse(os.Id, os.Numero);
    }
}
