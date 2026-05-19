namespace OficinaMecanica.Atendimento.Application.Ports.Out;

// Representa apenas o contrato de consulta de peças referenciadas na OS.
// A entidade Peca pertence ao microserviço Estoque — aqui só trafegam Guid e nome.
public interface IPecaRepository
{
    Task<bool> ExisteAsync(Guid pecaId, CancellationToken ct = default);
    Task<string?> ObterNomeAsync(Guid pecaId, CancellationToken ct = default);
}
