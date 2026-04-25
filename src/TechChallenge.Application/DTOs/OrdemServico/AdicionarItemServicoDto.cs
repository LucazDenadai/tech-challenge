using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.OrdemServico;

public class AdicionarItemServicoDto
{
    [Required]
    public Guid ServicoId { get; set; }
}
