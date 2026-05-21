using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using DomainServico = OficinaMecanica.Atendimento.Domain.Entities.Servico;

namespace OficinaMecanica.Atendimento.Application.UseCases.Catalogo;

public class GerenciarCatalogoUseCase(IServicoRepository servicoRepository)
{
    public async Task<IEnumerable<ServicoResponse>> ObterServicosAsync(CancellationToken ct = default)
    {
        var servicos = await servicoRepository.ObterAtivosAsync(ct);
        return servicos.Select(ToResponse);
    }

    public async Task<ServicoResponse?> ObterServicoPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var servico = await servicoRepository.ObterPorIdAsync(id, ct);
        return servico is null ? null : ToResponse(servico);
    }

    public async Task<ServicoResponse> CriarServicoAsync(CriarServicoRequest request, CancellationToken ct = default)
    {
        var servico = new DomainServico(request.Nome, request.Descricao, request.Preco, request.TempoConclusaoMinutos);
        await servicoRepository.AdicionarAsync(servico, ct);
        await servicoRepository.SalvarAsync(ct);
        return ToResponse(servico);
    }

    public async Task<ServicoResponse> AtualizarServicoAsync(AtualizarServicoRequest request, CancellationToken ct = default)
    {
        var servico = await servicoRepository.ObterPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Servico", request.Id);

        servico.Atualizar(request.Nome, request.Descricao, request.Preco, request.TempoConclusaoMinutos);
        await servicoRepository.AtualizarAsync(servico, ct);
        await servicoRepository.SalvarAsync(ct);
        return ToResponse(servico);
    }

    public async Task DesativarServicoAsync(Guid id, CancellationToken ct = default)
    {
        var servico = await servicoRepository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException("Servico", id);

        servico.Desativar();
        await servicoRepository.AtualizarAsync(servico, ct);
        await servicoRepository.SalvarAsync(ct);
    }

    private static ServicoResponse ToResponse(DomainServico s) =>
        new(s.Id, s.Nome, s.Descricao, s.Preco, s.TempoConclusaoMinutos, s.Ativo);
}
