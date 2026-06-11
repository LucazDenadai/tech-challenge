namespace OficinaMecanica.Atendimento.Application.Ports.Out;

public interface IEstoquePort
{
    Task<bool> VerificarDisponibilidadeAsync(IEnumerable<ItemPecaRequest> itens, CancellationToken ct = default);
    Task<PecaEstoqueDto?> ObterPecaAsync(Guid pecaId, CancellationToken ct = default);
}

public record ItemPecaRequest(Guid PecaId, int Quantidade);
public record PecaEstoqueDto(Guid Id, string Nome, string Descricao, decimal Valor);
