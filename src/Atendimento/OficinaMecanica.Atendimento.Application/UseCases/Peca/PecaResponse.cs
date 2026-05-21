namespace OficinaMecanica.Atendimento.Application.UseCases.Peca;

public record PecaResponse(Guid Id, string Nome, string Descricao, decimal Preco, bool Ativo);
