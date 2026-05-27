using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Atendimento.Application.UseCases.Catalogo;

public record CriarServicoRequest(
    [Required(AllowEmptyStrings = false)] string Nome,
    string Descricao,
    [Range(0.01, double.MaxValue)] decimal Preco,
    [Range(1, int.MaxValue)] int TempoConclusaoMinutos);

public record AtualizarServicoRequest(Guid Id, string Nome, string Descricao, decimal Preco, int TempoConclusaoMinutos);
