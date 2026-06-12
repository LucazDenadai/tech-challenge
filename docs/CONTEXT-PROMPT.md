# Prompt de contexto — Tech Challenge Fase 2

Cole este prompt no início de qualquer chat novo antes de pedir a execução de um card.

---

## Contexto do projeto

Estou desenvolvendo o **Tech Challenge Fase 2** de uma pós-graduação em Software Architecture. O projeto é um sistema de gestão de ordens de serviço para uma oficina mecânica.

### Stack
- ASP.NET Core .NET 10 + C#
- PostgreSQL + EF Core 9.x
- Docker + Kubernetes + Terraform
- GitHub Actions (CI/CD)
- RabbitMQ + MassTransit (mensageria)
- xUnit + Moq + Testcontainers (testes)

### Repositório
Monorepo Git em `d:\Dev\Tech-challenge\` com a seguinte estrutura planejada:

```
src/
├── Atendimento/
│   ├── OficinaMecanica.Atendimento.Domain/
│   ├── OficinaMecanica.Atendimento.Application/
│   ├── OficinaMecanica.Atendimento.Infrastructure/
│   └── OficinaMecanica.Atendimento.API/
└── Estoque/
    ├── OficinaMecanica.Estoque.Domain/
    ├── OficinaMecanica.Estoque.Application/
    ├── OficinaMecanica.Estoque.Infrastructure/
    └── OficinaMecanica.Estoque.API/
tests/
├── Atendimento/
│   ├── OficinaMecanica.Atendimento.UnitTests/
│   └── OficinaMecanica.Atendimento.IntegrationTests/
└── Estoque/
    ├── OficinaMecanica.Estoque.UnitTests/
    └── OficinaMecanica.Estoque.IntegrationTests/
k8s/
infra/
docs/
├── adr/
│   └── ADR-001-arquitetura-microservicos-mensageria.md
└── cards/
```

---

## Decisões arquiteturais já tomadas (não questionar)

### Arquitetura
- **Arquitetura Hexagonal** (Ports & Adapters) em cada microserviço
- **2 microserviços**: Atendimento (porta 8080) e Estoque (porta 8081)
- Comunicação **assíncrona via RabbitMQ** para baixa de estoque (evento `os.finalizada`)
- Comunicação **síncrona via HTTP** apenas na abertura de OS (verificar disponibilidade de peças)
- **Polly** para circuit breaker e retry na chamada HTTP entre serviços
- **MassTransit** como abstração do RabbitMQ

### Banco de dados
- 1 instância PostgreSQL com **2 schemas separados**: `atendimento` e `estoque`
- Migrations do EF Core copiadas do legado (não recriar do zero)

### Testes
- **TDD** nos use cases do Application (teste primeiro, implementação depois)
- **Testcontainers** nos testes de integração (PostgreSQL real, sem mock)

### Stubs
Os seguintes adapters são criados como stub primeiro e substituídos em cards posteriores:
- `EventPublisherStub` → substituído no CARD-09
- `EstoqueHttpStub` → substituído no CARD-10
- `EmailStub` → substituído no CARD-10

---

## Código legado (Fase 1)

Existe código funcional da Fase 1 em `/legacy` (ou nos projetos antigos ainda na solution). Contém:
- Entidades: `OrdemServico`, `Cliente`, `Veiculo`, `Servico`, `Peca`, `ItemServico`, `ItemPeca`, `HistoricoStatusOS`, `Usuario`
- Enums: `StatusOrdemServico`, `PerfilUsuario`
- Validators: `CpfValidator`, `CnpjValidator`, `PlacaValidator`
- 3 migrations EF Core: `InitialCreate`, `AddHistoricoStatusOS`, `RenameCpfToDocumento`
- Testes unitários e de integração existentes

**Importante:** reutilizar o que faz sentido, não reescrever por reescrever.

---

## Regras que devem ser seguidas em todos os cards

1. **Domain** não tem nenhum `using` externo — sem EF Core, sem BCrypt, sem JWT
2. **Application** não referencia Infrastructure — só define interfaces (portas) e use cases
3. Um **use case por fluxo** — sem classes "service" genéricas que acumulam responsabilidades
4. Controllers são **adapters de entrada** — sem lógica de negócio, só delegam ao use case
5. **TDD nos use cases**: escrever o teste, ver falhar, implementar, ver passar
6. **Nenhuma credencial hardcoded** — tudo via variáveis de ambiente ou Secrets K8s
7. `dotnet build` sem warnings antes de considerar qualquer card concluído

---

## Cards e ordem de execução

Todos os cards estão detalhados em `docs/cards/`. A ordem de dependência é:

```
CARD-01 → CARD-02 → CARD-03 → CARD-04 ─┐
                              CARD-05 ──┴→ CARD-06 → CARD-07
CARD-01 ──────────── CARD-08 ──────────── CARD-09 → CARD-10
                     CARD-11 ──────────── CARD-12 → CARD-13
                                          CARD-14 → CARD-15
```

### Resumo dos cards

| Card | Épico | Título |
|---|---|---|
| CARD-01 | Hexagonal | Scaffolding da solution monorepo |
| CARD-02 | Hexagonal | Domain layer do Atendimento |
| CARD-03 | Hexagonal | Application: portas de saída |
| CARD-04 | Hexagonal | Application: use cases (TDD) |
| CARD-05 | Hexagonal | Infrastructure: adapters de saída + migrations |
| CARD-06 | Hexagonal | API: adapters de entrada (controllers) |
| CARD-07 | Hexagonal | Testes de integração do Atendimento |
| CARD-08 | Estoque | Microserviço de Estoque |
| CARD-09 | Estoque | RabbitMQ: publisher + consumer |
| CARD-10 | Estoque | Adapters reais: EstoqueHttp + Email |
| CARD-11 | Infra | Docker: Dockerfile + docker-compose |
| CARD-12 | Infra | Kubernetes: manifestos YAML |
| CARD-13 | Infra | Terraform: infraestrutura como código |
| CARD-14 | CI/CD | Pipeline GitHub Actions |
| CARD-15 | CI/CD | README.md do repositório |

---

## Como usar este prompt

1. Cole este arquivo inteiro no início do chat
2. Adicione na sequência: **"Execute o CARD-XX"** ou **"Vamos trabalhar no CARD-XX"**
3. Cole também o conteúdo do card específico de `docs/cards/`
4. O assistente terá contexto completo para executar sem ambiguidade

---

## Experiências técnicas — lições aprendidas

### CARD-14: Deploy Kubernetes

**Connection string com `localhost` no cluster**
O secret do GitHub Actions (`ATENDIMENTO_CONNECTION_STRING`) estava com `Host=localhost` — funciona localmente mas quebra no k8s onde o postgres é um Service separado. O host correto é o nome do Service: `postgres-svc`.

**Nome de chave errado no secret**
O workflow aplicava `ConnectionStrings__Default` mas o código lê `ConnectionStrings__DefaultConnection` (padrão do `GetConnectionString("DefaultConnection")`). Resultado: connection string nunca era lida, caía no fallback `POSTGRES_CONNECTION` que não existia e lançava exception.

**Banco não existia no PostgreSQL**
O EF Core `MigrateAsync()` cria tabelas mas não cria o banco. A solução foi adicionar no startup de cada API um bloco que conecta no banco `postgres`, verifica se o banco alvo existe e o cria se necessário — antes de chamar `MigrateAsync()`.

**Pipeline rodando jobs em paralelo sem sentido**
Os jobs `build-and-test`, `docker` e `deploy` rodavam em paralelo. A ordem correta é sequencial: `build-and-test → docker → deploy`. Corrigido com `needs:` em cada job.

---

## Experiências técnicas — lições aprendidas

### CARD-14: Deploy Kubernetes

**Connection string com `localhost` no cluster**
O secret do GitHub Actions estava com `Host=localhost` — funciona localmente mas quebra no k8s onde o postgres é um Service separado. O host correto é o nome do Service: `postgres-svc`.

**Nome de chave errado no secret**
O workflow aplicava `ConnectionStrings__Default` mas o código lê `ConnectionStrings__DefaultConnection` (padrão do `GetConnectionString("DefaultConnection")`). A connection string nunca era lida, caía no fallback e lançava exception.

**Banco não existia no PostgreSQL**
O EF Core `MigrateAsync()` cria tabelas mas não cria o banco. Solução: no startup de cada API, conectar no banco `postgres`, verificar se o banco alvo existe e criá-lo se necessário — antes de chamar `MigrateAsync()`.

**Pipeline sem ordenação entre jobs**
Os jobs `build-and-test`, `docker` e `deploy` rodavam em paralelo. A ordem correta é sequencial via `needs:`: `build-and-test → docker → deploy`.

---

## ADR de referência

A decisão de arquitetura completa com todas as alternativas consideradas está em:
`docs/adr/ADR-001-arquitetura-microservicos-mensageria.md`
