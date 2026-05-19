namespace OficinaMecanica.Atendimento.Application.UseCases.Catalogo;

public record CriarServicoRequest(string Nome, string Descricao, decimal Preco, int TempoConclusaoMinutos);
public record AtualizarServicoRequest(Guid Id, string Nome, string Descricao, decimal Preco, int TempoConclusaoMinutos);
