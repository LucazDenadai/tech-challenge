using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public class AdicionarServicoOSRequest
{
    [Required(ErrorMessage = "O ServicoId é obrigatório.")]
    public Guid ServicoId { get; set; }
}

public class AdicionarPecaOSRequest
{
    [Required(ErrorMessage = "O PecaId é obrigatório.")]
    public Guid PecaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}
