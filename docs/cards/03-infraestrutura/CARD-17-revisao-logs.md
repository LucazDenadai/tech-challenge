# CARD-17 — Revisão de logs estruturados

**Tipo:** Aplicação  
**Status:** To Do  
**Depende de:** nenhum  
**Bloqueia:** CARD-16 (Observabilidade — logs precisam estar estruturados antes de integrar OTel)

---

## Contexto

OpenTelemetry propaga automaticamente o `TraceId` entre serviços via headers HTTP e mensagens de fila. Para que esse contexto apareça nos logs e permita correlacionar um log com um trace no Jaeger, os logs precisam ser **estruturados** e incluir o `TraceId` no payload.

Logs do tipo `Console.WriteLine("Erro ao processar OS")` não carregam contexto e são inúteis para diagnóstico distribuído. Logs estruturados têm a forma:

```json
{
  "Timestamp": "2026-06-10T14:32:01Z",
  "Level": "Error",
  "Message": "Falha ao processar ordem de servico",
  "TraceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "SpanId": "00f067aa0ba902b7",
  "OrdemServicoId": "abc-123",
  "Exception": "..."
}
```

Com isso, ao ver um erro no Grafana ou Prometheus, é possível colar o `TraceId` no Jaeger e ver exatamente o que aconteceu.

---

## Critérios de aceite

- [ ] Levantar inventário de todos os pontos de log nos dois serviços (Atendimento e Estoque)
- [ ] Classificar cada ponto: adequado / ausente / inadequado (ex: só `Console.WriteLine`)
- [ ] Garantir que `ILogger<T>` é usado em todos os use cases e consumers (não `Console.Write`)
- [ ] Garantir que eventos de domínio relevantes têm log: criação de OS, mudança de status, consumo de estoque, falha de processamento
- [ ] Garantir que erros são logados com `LogError` (não `LogInformation`)
- [ ] Confirmar que o formato de saída está em JSON estruturado (via `AddJsonConsole` ou Serilog)
- [ ] `TraceId` e `SpanId` aparecem automaticamente nos logs após integração com OTel (validar no CARD-16)
- [ ] Nenhum dado sensível (senha, token JWT, connection string) em nenhum log

---

## O que verificar (checklist de revisão)

### Formato de saída
- O projeto usa `AddJsonConsole()` no `Program.cs`? Ou Serilog com sink JSON?
- Se estiver usando o formato padrão do ASP.NET (texto plano), migrar para JSON estruturado

### Cobertura de eventos de negócio

| Evento | Serviço | Log esperado |
|---|---|---|
| OS criada | Atendimento | `LogInformation` com `OrdemServicoId` |
| Status de OS alterado | Atendimento | `LogInformation` com status anterior e novo |
| Orçamento aprovado/recusado | Atendimento | `LogInformation` com decisão |
| Mensagem publicada no RabbitMQ | Atendimento | `LogInformation` com tipo de evento |
| Mensagem consumida do RabbitMQ | Estoque | `LogInformation` com tipo de evento |
| Estoque consumido com sucesso | Estoque | `LogInformation` com peça e quantidade |
| Falha ao consumir mensagem | Estoque | `LogError` com detalhes da falha |
| Falha de processamento registrada | Estoque | `LogWarning` com contexto |

### Uso de `ILogger<T>`
- Use cases injetam `ILogger<T>` via construtor?
- Consumers do MassTransit logam início, sucesso e falha do processamento?
- Controllers **não** devem logar lógica de negócio — isso é responsabilidade do use case

### Dados sensíveis
- Nenhum `LogDebug` ou `LogInformation` expõe senha, token ou connection string
- Revisar especialmente o código de autenticação (`AuthController`, `LoginUseCase`)

---

## Estrutura esperada de log num use case

```csharp
public class CriarOrdemServicoUseCase
{
    private readonly ILogger<CriarOrdemServicoUseCase> _logger;

    public async Task<OrdemServico> ExecutarAsync(CriarOrdemServicoCommand command)
    {
        _logger.LogInformation("Criando ordem de servico para veiculo {VeiculoId}", command.VeiculoId);

        // ... lógica ...

        _logger.LogInformation("Ordem de servico {OrdemServicoId} criada com sucesso", os.Id);
        return os;
    }
}
```

---

## Passos

1. Listar todos os arquivos de use cases e consumers nos dois serviços
2. Para cada arquivo, verificar se usa `ILogger<T>` e quais eventos são logados
3. Registrar achados: adequado / ausente / inadequado
4. Corrigir casos inadequados (substituir `Console.Write`, adicionar logs ausentes em eventos críticos)
5. Verificar configuração de formato JSON no `Program.cs` de cada serviço
6. Verificar ausência de dados sensíveis nos logs
7. Documentar no PR os pontos alterados e os que já estavam adequados
