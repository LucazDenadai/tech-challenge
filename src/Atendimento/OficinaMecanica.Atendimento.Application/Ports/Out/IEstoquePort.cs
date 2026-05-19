namespace OficinaMecanica.Atendimento.Application.Ports.Out;

// TODO: implementado no CARD-09 (HTTP com Polly para o microserviço Estoque)
public interface IEstoquePort
{
    Task<bool> VerificarDisponibilidadeAsync(IEnumerable<ItemPecaRequest> itens, CancellationToken ct = default);
}

public record ItemPecaRequest(Guid PecaId, int Quantidade);
