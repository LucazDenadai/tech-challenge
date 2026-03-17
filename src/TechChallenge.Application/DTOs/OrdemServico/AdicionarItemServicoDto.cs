using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.OrdemServico;

public class AdicionarItemServicoDto
{
    [Required]
    public Guid ServicoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; } = 1;
}
