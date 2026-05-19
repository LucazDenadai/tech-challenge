namespace TechChallenge.Application.DTOs.Veiculo;

public class VeiculoDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public string Cor { get; set; } = string.Empty;
}
