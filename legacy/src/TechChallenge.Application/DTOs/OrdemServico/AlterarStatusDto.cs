using System.ComponentModel.DataAnnotations;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.DTOs.OrdemServico;

public class AlterarStatusDto
{
    [Required]
    public StatusOrdemServico NovoStatus { get; set; }
}
