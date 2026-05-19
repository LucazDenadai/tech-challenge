namespace TechChallenge.Application.DTOs.Servico;

public class ServicoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int TempoConclusaoMinutos { get; set; }
    public bool Ativo { get; set; }
}
