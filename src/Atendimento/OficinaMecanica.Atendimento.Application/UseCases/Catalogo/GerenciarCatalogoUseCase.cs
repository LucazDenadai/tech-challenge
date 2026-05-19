using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using DomainServico = OficinaMecanica.Atendimento.Domain.Entities.Servico;

namespace OficinaMecanica.Atendimento.Application.UseCases.Catalogo;

public class GerenciarCatalogoUseCase(IServicoRepository servicoRepository)
{
    public async Task<Guid> CriarServicoAsync(CriarServicoRequest request, CancellationToken ct = default)
    {
        var servico = new DomainServico(request.Nome, request.Descricao, request.Preco, request.TempoConclusaoMinutos);
        await servicoRepository.AdicionarAsync(servico, ct);
        await servicoRepository.SalvarAsync(ct);
        return servico.Id;
    }

    public async Task AtualizarServicoAsync(AtualizarServicoRequest request, CancellationToken ct = default)
    {
        var servico = await servicoRepository.ObterPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Servico", request.Id);

        servico.Atualizar(request.Nome, request.Descricao, request.Preco, request.TempoConclusaoMinutos);
        await servicoRepository.AtualizarAsync(servico, ct);
        await servicoRepository.SalvarAsync(ct);
    }

    public async Task DesativarServicoAsync(Guid id, CancellationToken ct = default)
    {
        var servico = await servicoRepository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException("Servico", id);

        servico.Desativar();
        await servicoRepository.AtualizarAsync(servico, ct);
        await servicoRepository.SalvarAsync(ct);
    }
}
