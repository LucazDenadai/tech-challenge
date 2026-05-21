using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;

public record AdicionarServicoOSRequest([property: Required] Guid ServicoId);
public record AdicionarPecaOSRequest([property: Required] Guid PecaId, [property: Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")] int Quantidade);
