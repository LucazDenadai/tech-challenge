using OficinaMecanica.Atendimento.Application.Exceptions;
using OficinaMecanica.Atendimento.Application.Ports.Out;
using DomainPeca = OficinaMecanica.Atendimento.Domain.Entities.Peca;

namespace OficinaMecanica.Atendimento.Application.UseCases.Peca;

public class GerenciarPecaUseCase(IPecaRepository repository)
{
    public async Task<IEnumerable<PecaResponse>> ObterTodosAsync(CancellationToken ct = default)
    {
        var pecas = await repository.ObterTodosAsync(ct);
        return pecas.Select(ToResponse);
    }

    public async Task<IEnumerable<PecaResponse>> BuscarPorNomeAsync(string nome, CancellationToken ct = default)
    {
        var pecas = await repository.BuscarPorNomeAsync(nome, ct);
        return pecas.Select(ToResponse);
    }

    public async Task<PecaResponse?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var peca = await repository.ObterPorIdAsync(id, ct);
        return peca is null ? null : ToResponse(peca);
    }

    public async Task<PecaResponse> CriarAsync(CriarPecaRequest request, CancellationToken ct = default)
    {
        var peca = new DomainPeca(request.Nome, request.Descricao, request.Preco);
        await repository.AdicionarAsync(peca, ct);
        await repository.SalvarAsync(ct);
        return ToResponse(peca);
    }

    public async Task<PecaResponse> AtualizarAsync(Guid id, AtualizarPecaRequest request, CancellationToken ct = default)
    {
        var peca = await repository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException("Peca", id);

        peca.Atualizar(request.Nome, request.Descricao, request.Preco);
        await repository.AtualizarAsync(peca, ct);
        await repository.SalvarAsync(ct);
        return ToResponse(peca);
    }

    public async Task DesativarAsync(Guid id, CancellationToken ct = default)
    {
        var peca = await repository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException("Peca", id);

        peca.Desativar();
        await repository.AtualizarAsync(peca, ct);
        await repository.SalvarAsync(ct);
    }

    private static PecaResponse ToResponse(DomainPeca p) =>
        new(p.Id, p.Nome, p.Descricao, p.Preco, p.Ativo);
}
