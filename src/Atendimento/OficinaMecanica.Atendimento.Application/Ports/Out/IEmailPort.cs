using OficinaMecanica.Atendimento.Domain.Enums;

namespace OficinaMecanica.Atendimento.Application.Ports.Out;

// TODO: implementado no CARD-10 (adapter de email via SMTP/SendGrid)
public interface IEmailPort
{
    Task EnviarAtualizacaoStatusAsync(string destinatario, string numeroOS, StatusOrdemServico novoStatus, CancellationToken ct = default);
}
