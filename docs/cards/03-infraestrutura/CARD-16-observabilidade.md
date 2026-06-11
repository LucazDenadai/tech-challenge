# CARD-16 — Observabilidade: OpenTelemetry + Jaeger + Prometheus + Grafana

**Tipo:** Infra / Aplicação  
**Status:** To Do  
**Depende de:** CARD-13 (namespace `observabilidade` provisionado), CARD-17 (logs revisados)  
**Bloqueia:** nenhum

---

## Contexto

Com os dois microsserviços em produção no Kubernetes, precisamos de visibilidade sobre o comportamento do sistema em tempo real. Sem observabilidade, falhas silenciosas (ex: mensagens perdidas no RabbitMQ, latência alta entre serviços) são invisíveis até o usuário reportar.

Os três pilares da observabilidade são cobridos por esta stack:

| Pilar | Ferramenta | O que responde |
|---|---|---|
| **Traces** | OpenTelemetry + Jaeger | "Onde essa requisição foi lenta ou falhou?" |
| **Métricas** | Prometheus + Grafana | "Como o sistema está se comportando ao longo do tempo?" |
| **Logs** | Estruturados via OTel (ver CARD-17) | "O que aconteceu neste contexto específico?" |

---

## Critérios de aceite

### Instrumentação (código)
- [ ] SDK OpenTelemetry instalado em `OficinaMecanica.Atendimento.API` e `OficinaMecanica.Estoque.API`
- [ ] Traces automáticos de requisições HTTP (via `AddAspNetCoreInstrumentation`)
- [ ] Traces automáticos de queries SQL via EF Core (via `AddEntityFrameworkCoreInstrumentation`)
- [ ] Traces automáticos de mensagens RabbitMQ/MassTransit (via `AddMassTransitInstrumentation`)
- [ ] `TraceId` propagado entre Atendimento e Estoque via headers HTTP e mensagens do broker
- [ ] Métricas HTTP exportadas para Prometheus (via `AddPrometheusExporter`)

### Infraestrutura (K8s)
- [ ] Jaeger rodando no namespace `observabilidade` (all-in-one para demo)
- [ ] Prometheus rodando no namespace `observabilidade` com scrape configurado para os serviços
- [ ] Grafana rodando com datasource Prometheus configurado
- [ ] Dashboard básico no Grafana: latência p50/p95, taxa de erro, requisições/s
- [ ] Services K8s expostos via NodePort para acesso local

### Validação
- [ ] Trace de uma ordem de serviço completa visível no Jaeger (Atendimento → RabbitMQ → Estoque)
- [ ] Métricas de Atendimento e Estoque visíveis no Prometheus
- [ ] Dashboard no Grafana mostrando dados reais

---

## O que é cada ferramenta (didático)

### OpenTelemetry (OTel)
É o SDK que instrumenta o código. Ele captura automaticamente spans (unidades de trace) para cada requisição HTTP, query SQL e mensagem de fila, sem precisar adicionar código em cada endpoint. Você configura uma vez no `Program.cs` e ele instrumenta tudo.

### Jaeger
É o backend de **traces distribuídos**. Armazena e visualiza o caminho completo de uma requisição entre serviços. Você consegue ver: "essa requisição de criar OS levou 230ms — 20ms na API, 180ms no banco, 30ms para publicar no RabbitMQ".

### Prometheus
É o banco de dados de **métricas**. Periodicamente "raspa" (scrape) os endpoints `/metrics` dos serviços e armazena séries temporais. Responde perguntas como: "quantas requisições por segundo o Atendimento está recebendo agora?"

### Grafana
É a camada de **visualização**. Conecta no Prometheus e exibe dashboards com gráficos. Também suporta alertas (fora do escopo deste card).

---

## Estrutura de arquivos

```
k8s/
└── observabilidade/
    ├── namespace.yaml       ← (já criado pelo Terraform/CARD-13)
    ├── jaeger.yaml          ← Deployment + Service (NodePort)
    ├── prometheus/
    │   ├── deployment.yaml
    │   ├── service.yaml
    │   └── configmap.yaml   ← scrape config apontando para os serviços
    └── grafana/
        ├── deployment.yaml
        ├── service.yaml
        └── configmap.yaml   ← datasource Prometheus pré-configurado

src/
├── Atendimento/OficinaMecanica.Atendimento.API/
│   └── Extensions/
│       └── OpenTelemetryExtensions.cs   ← configuração do SDK OTel
└── Estoque/OficinaMecanica.Estoque.API/
    └── Extensions/
        └── OpenTelemetryExtensions.cs
```

---

## Pacotes NuGet necessários (por serviço)

```
OpenTelemetry
OpenTelemetry.Extensions.Hosting
OpenTelemetry.Instrumentation.AspNetCore
OpenTelemetry.Instrumentation.EntityFrameworkCore
OpenTelemetry.Instrumentation.MassTransit   ← ou OpenTelemetry.Instrumentation.Quartz
OpenTelemetry.Exporter.Jaeger
OpenTelemetry.Exporter.Prometheus.AspNetCore
```

---

## Configuração no Program.cs (exemplo)

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("OficinaMecanica.Atendimento"))
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddMassTransitInstrumentation()
        .AddJaegerExporter(o =>
        {
            o.AgentHost = builder.Configuration["Jaeger:Host"] ?? "localhost";
            o.AgentPort = 6831;
        }))
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("OficinaMecanica.Atendimento"))
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());

// Expõe /metrics para o Prometheus raspar
app.MapPrometheusScrapingEndpoint();
```

---

## Impacto no CARD-14 (CI/CD)

O `cd.yml` precisa aplicar os manifestos de observabilidade no deploy:

```yaml
- name: Aplicar stack de observabilidade
  run: kubectl apply -f k8s/observabilidade/ -n observabilidade
```

Isso é adicionado **após** o deploy dos serviços de aplicação e não bloqueia o rollout deles.

---

## Passos

1. Adicionar pacotes NuGet de OTel nos dois projetos de API
2. Criar `OpenTelemetryExtensions.cs` e registrar no `Program.cs` de cada serviço
3. Criar manifestos K8s para Jaeger (all-in-one)
4. Criar manifestos K8s para Prometheus com scrape config
5. Criar manifestos K8s para Grafana com datasource pré-configurado
6. Adicionar variável `Jaeger__Host` nos ConfigMaps dos serviços
7. Validar traces no Jaeger com uma requisição end-to-end
8. Validar métricas no Prometheus (`/metrics` dos serviços)
9. Criar dashboard básico no Grafana e exportar como JSON (commitar em `k8s/observabilidade/grafana/`)
