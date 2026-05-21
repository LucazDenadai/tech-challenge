using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Atendimento.Application.UseCases.Catalogo;

public class CriarServicoRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string Descricao { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "O tempo de conclusão deve ser maior que zero.")]
    public int TempoConclusaoMinutos { get; set; }
}

public record AtualizarServicoRequest(Guid Id, string Nome, string Descricao, decimal Preco, int TempoConclusaoMinutos);
