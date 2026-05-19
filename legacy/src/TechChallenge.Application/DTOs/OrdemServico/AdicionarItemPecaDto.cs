using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.OrdemServico;

public class AdicionarItemPecaDto
{
    [Required]
    public Guid PecaId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; } = 1;
}
