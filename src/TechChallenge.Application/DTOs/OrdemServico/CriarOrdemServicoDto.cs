using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Application.DTOs.OrdemServico;

public class CriarOrdemServicoDto
{
    /// <summary>CPF (11 dígitos) ou CNPJ (14 dígitos) do cliente</summary>
    [Required]
    public string DocumentoCliente { get; set; } = string.Empty;

    /// <summary>Placa do veículo (formato ABC1234 ou Mercosul ABC1D23)</summary>
    [Required]
    public string PlacaVeiculo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Observacoes { get; set; } = string.Empty;
}
