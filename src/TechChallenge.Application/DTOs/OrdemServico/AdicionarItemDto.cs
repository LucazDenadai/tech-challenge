using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TechChallenge.Application.DTOs.OrdemServico;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoItem
{
    Servico,
    Peca
}

public class AdicionarItemDto
{
    [Required]
    public TipoItem Tipo { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; } = 1;
}
