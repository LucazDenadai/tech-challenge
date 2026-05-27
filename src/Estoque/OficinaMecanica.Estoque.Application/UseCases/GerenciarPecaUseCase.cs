using OficinaMecanica.Estoque.Application.Exceptions;
using OficinaMecanica.Estoque.Application.Ports.Out;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Application.UseCases;

public record CriarPecaRequest(string Nome, string Descricao, decimal Valor, int QuantidadeEstoque);
public record AtualizarPecaRequest(Guid Id, string Nome, string Descricao, decimal Valor);
public record PecaResponse(Guid Id, string Nome, string Descricao, decimal Valor, int QuantidadeEstoque);

public class GerenciarPecaUseCase(IPecaRepository pecaRepository)
{
    public async Task<PecaResponse> CriarAsync(CriarPecaRequest request, CancellationToken ct = default)
    {
        var peca = new Peca(request.Nome, request.Descricao, request.Valor, request.QuantidadeEstoque);
        await pecaRepository.AdicionarAsync(peca, ct);
        await pecaRepository.SalvarAsync(ct);
        return ToResponse(peca);
    }

    public async Task<IEnumerable<PecaResponse>> ObterTodosAsync(CancellationToken ct = default)
    {
        var pecas = await pecaRepository.ObterTodosAsync(ct);
        return pecas.Select(ToResponse);
    }

    public async Task<PecaResponse?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var peca = await pecaRepository.ObterPorIdAsync(id, ct);
        return peca is null ? null : ToResponse(peca);
    }

    public async Task AtualizarAsync(AtualizarPecaRequest request, CancellationToken ct = default)
    {
        var peca = await pecaRepository.ObterPorIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Peca), request.Id);

        peca.Atualizar(request.Nome, request.Descricao, request.Valor);
        await pecaRepository.AtualizarAsync(peca, ct);
        await pecaRepository.SalvarAsync(ct);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var peca = await pecaRepository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Peca), id);

        await pecaRepository.RemoverAsync(peca, ct);
        await pecaRepository.SalvarAsync(ct);
    }

    private static PecaResponse ToResponse(Peca p) =>
        new(p.Id, p.Nome, p.Descricao, p.Valor, p.QuantidadeEstoque);
}
