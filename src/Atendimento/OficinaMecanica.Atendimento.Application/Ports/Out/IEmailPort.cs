using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.Ports.Out;

public interface IEmailPort
{
    Task EnviarAtualizacaoStatusAsync(string destinatario, string numeroOS, StatusOrdemServico novoStatus, CancellationToken ct = default);
}
