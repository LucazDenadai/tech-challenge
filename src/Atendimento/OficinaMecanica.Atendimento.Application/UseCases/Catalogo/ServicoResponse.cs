namespace OficinaMecanica.Atendimento.Application.UseCases.Catalogo;

public record ServicoResponse(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int TempoConclusaoMinutos,
    bool Ativo);
