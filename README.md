# Tech Challenge — Oficina Mecânica

Sistema de gerenciamento de oficina mecânica construído com arquitetura de **microsserviços hexagonal**, comunicação assíncrona via **RabbitMQ/MassTransit** e persistência em **PostgreSQL**.

Projeto acadêmico da pós-graduação em Software Architecture — FIAP.

---

## Arquitetura

```
┌──────────────────────────────────┐     evento RabbitMQ      ┌──────────────────────────────┐
│   Microserviço Atendimento       │ ─── OsFinalizadaEvent ──► │   Microserviço Estoque       │
│   porta 8080                     │                           │   porta 8081                 │
│                                  │                           │                              │
│  Hexagonal (Ports & Adapters)    │                           │  Hexagonal (Ports & Adapters)│
│  ├── Domain                      │                           │  ├── Domain                  │
│  ├── Application (use cases)     │                           │  ├── Application (use cases) │
│  ├── Infrastructure              │                           │  ├── Infrastructure          │
│  └── API (controllers)          │                           │  └── API (controllers)       │
└──────────────────────────────────┘                           └──────────────────────────────┘
              │                                                              │
              └──────────────────────┬───────────────────────────────────────┘
                                     │
                          ┌──────────▼──────────┐
                          │     PostgreSQL       │
                          │  oficina_atendimento │
                          │  oficina_estoque     │
                          └─────────────────────┘
```

### Fluxo de mensageria

Quando uma Ordem de Serviço é finalizada:

```
PATCH /ordens-servico/{id}/status → Finalizada
        │
        ▼ AtualizarStatusOSUseCase
        │  monta OsFinalizadaEvent { osId, itens[] }
        │
        ▼ RabbitMqEventPublisher
        │  publica no exchange "OsFinalizadaEvent"
        │
        ▼ Queue: estoque.baixa  (RabbitMQ)
        │
        ▼ BaixaEstoqueConsumer
           chama BaixarEstoqueUseCase
           → verifica idempotência (evita baixa dupla)
           → subtrai estoque de cada peça
           → registra MovimentacaoEstoque
```

Em caso de falha no consumer, o MassTransit faz **retry automático** (3 tentativas: 1s → 5s → 10s). Após esgotar, a mensagem vai para a queue `estoque.baixa_error`.

---

## Tecnologias

| Categoria | Tecnologia |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| Linguagem | C# |
| ORM | Entity Framework Core 9 + Npgsql |
| Banco de dados | PostgreSQL 16 |
| Mensageria | RabbitMQ 3 + MassTransit 8 |
| Autenticação | JWT (HMAC-SHA256) + BCrypt |
| Documentação | Swagger / Swashbuckle |
| Containerização | Docker + Docker Compose |
| Testes | xUnit + Moq + FluentAssertions + Testcontainers |
| Cobertura | coverlet (OpenCover) |
| Qualidade | SonarAnalyzer for C# |

---

## Estrutura do projeto

```
src/
├── Atendimento/
│   ├── OficinaMecanica.Atendimento.Domain/          # Entidades, enums, regras de negócio
│   ├── OficinaMecanica.Atendimento.Application/     # Use cases, ports (interfaces), eventos
│   ├── OficinaMecanica.Atendimento.Infrastructure/  # EF Core, repositórios, RabbitMQ publisher, stubs
│   └── OficinaMecanica.Atendimento.API/             # Controllers, filtros, Program.cs
│
└── Estoque/
    ├── OficinaMecanica.Estoque.Domain/              # Entidades Peca e MovimentacaoEstoque
    ├── OficinaMecanica.Estoque.Application/         # Use cases, ports, eventos
    ├── OficinaMecanica.Estoque.Infrastructure/      # EF Core, repositórios, BaixaEstoqueConsumer
    └── OficinaMecanica.Estoque.API/                 # Controllers, Program.cs

tests/
├── Atendimento/
│   ├── OficinaMecanica.Atendimento.UnitTests/       # Testes unitários dos use cases
│   └── OficinaMecanica.Atendimento.IntegrationTests/# HTTP + PostgreSQL real (Testcontainers)
└── Estoque/
    ├── OficinaMecanica.Estoque.UnitTests/           # Testes unitários dos use cases
    └── OficinaMecanica.Estoque.IntegrationTests/    # HTTP + PostgreSQL real (Testcontainers)

docs/
├── adr/                                             # Architecture Decision Records
└── cards/                                           # Cartões de implementação por sprint
```

---

## Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para rodar tudo com compose)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (para desenvolvimento local e testes)

---

## Como executar

### 1. Configure o arquivo `.env`

Copie o exemplo e ajuste se necessário:

```bash
cp .env.example .env
```

O `.env.example` já tem valores prontos para desenvolvimento local.

### 2. Suba tudo com Docker Compose

```bash
docker compose up --build
```

Isso sobe em ordem: PostgreSQL → RabbitMQ → Atendimento API → Estoque API.

### Serviços disponíveis

| Serviço | URL |
|---|---|
| Atendimento API | http://localhost:8080 |
| Atendimento Swagger | http://localhost:8080/swagger |
| Estoque API | http://localhost:8081 |
| Estoque Swagger | http://localhost:8081/swagger |
| RabbitMQ Management UI | http://localhost:15672 (guest / guest) |

---

## Observabilidade

### Logs dos containers

Ver logs em tempo real de todos os serviços:

```bash
docker compose logs -f
```

Filtrar por serviço específico:

```bash
docker compose logs -f api           # Atendimento
docker compose logs -f estoque-api   # Estoque
docker compose logs -f rabbitmq      # RabbitMQ
```

### O que cada serviço loga

**Atendimento** — ao finalizar uma OS:
```
info: RabbitMqEventPublisher — publicando OsFinalizadaEvent osId=... itens=2
```

**Estoque** — ao consumir o evento:
```
info: MassTransit — Received message OsFinalizadaEvent
info: BaixaEstoqueConsumer — baixa processada osId=... 2 itens
```

**Estoque** — se o consumer falhar (retry):
```
warn: MassTransit — Retry 1/3 for OsFinalizadaEvent after 1000ms
warn: MassTransit — Retry 2/3 for OsFinalizadaEvent after 5000ms
error: MassTransit — Message moved to estoque.baixa_error after 3 retries
```

### RabbitMQ Management UI

Acesse http://localhost:15672 com `guest` / `guest` para visualizar:

- **Queues** → `estoque.baixa` — mensagens pendentes e taxa de processamento
- **Queues** → `estoque.baixa_error` — mensagens que falharam após 3 tentativas (dead letter)
- **Exchanges** → `OficinaMecanica.Estoque.Application.Events:OsFinalizadaEvent` — exchange criado pelo MassTransit
- **Overview** → throughput global de mensagens

Para **reprocessar** uma mensagem da fila de erro, use o botão "Move messages" na UI do RabbitMQ apontando de `estoque.baixa_error` para `estoque.baixa`.

---

## Autenticação (Atendimento API)

A API do Atendimento usa JWT. Para acessar endpoints protegidos:

**1. Faça login:**

```http
POST /auth/login
{
  "email": "admin@oficina.com",
  "senha": "Admin@123"
}
```

**2. Use o token no header:**

```
Authorization: Bearer {token}
```

### Usuário padrão (criado automaticamente na primeira subida)

| Email | Senha | Perfil |
|---|---|---|
| admin@oficina.com | Admin@123 | Admin |

---

## Endpoints

### Atendimento API — `localhost:8080`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| POST | `/auth/login` | Login e obtenção de token | — |
| GET | `/clientes` | Listar clientes | JWT |
| POST | `/clientes` | Criar cliente | JWT (Admin, Atendente) |
| PUT | `/clientes/{id}` | Atualizar cliente | JWT (Admin, Atendente) |
| DELETE | `/clientes/{id}` | Desativar cliente | JWT (Admin) |
| GET | `/veiculos` | Listar veículos | JWT |
| POST | `/veiculos` | Criar veículo | JWT (Admin, Atendente) |
| GET | `/servicos` | Listar serviços do catálogo | JWT |
| POST | `/servicos` | Criar serviço | JWT (Admin) |
| GET | `/pecas` | Listar peças do catálogo | JWT |
| POST | `/pecas` | Criar peça | JWT (Admin) |
| GET | `/ordens-servico` | Listar OSs com filtros | JWT |
| POST | `/ordens-servico` | Abrir OS | JWT (Admin, Atendente) |
| GET | `/ordens-servico/{id}` | Detalhe da OS | JWT |
| PATCH | `/ordens-servico/{id}/status` | Avançar status da OS | JWT |
| POST | `/ordens-servico/{id}/itens` | Adicionar item (peça/serviço) | JWT (Mecânico) |
| DELETE | `/ordens-servico/{id}/itens/{itemId}` | Cancelar item | JWT (Mecânico) |
| GET | `/ordens-servico/acompanhar/{numero}` | Status público para o cliente | — |

### Estoque API — `localhost:8081`

| Método | Rota | Descrição |
|---|---|---|
| GET | `/estoque/pecas` | Listar peças do estoque |
| GET | `/estoque/pecas/{id}` | Detalhe de uma peça |
| POST | `/estoque/pecas` | Cadastrar peça no estoque |
| PUT | `/estoque/pecas/{id}` | Atualizar peça |
| DELETE | `/estoque/pecas/{id}` | Remover peça |
| POST | `/estoque/disponibilidade` | Verificar disponibilidade de itens |

---

## Testes

### Rodar todos os testes

```bash
dotnet test
```

Saída esperada:
```
Passed! - Failed: 0, Passed: 30  - OficinaMecanica.Atendimento.UnitTests
Passed! - Failed: 0, Passed: 11  - OficinaMecanica.Estoque.UnitTests
Passed! - Failed: 0, Passed: 58  - OficinaMecanica.Atendimento.IntegrationTests
Passed! - Failed: 0, Passed:  8  - OficinaMecanica.Estoque.IntegrationTests
```

> Os testes de integração requerem **Docker em execução** — o Testcontainers sobe PostgreSQL automaticamente.

### Testes por projeto

```bash
# Unitários
dotnet test tests/Atendimento/OficinaMecanica.Atendimento.UnitTests/
dotnet test tests/Estoque/OficinaMecanica.Estoque.UnitTests/

# Integração
dotnet test tests/Atendimento/OficinaMecanica.Atendimento.IntegrationTests/
dotnet test tests/Estoque/OficinaMecanica.Estoque.IntegrationTests/
```

### Com relatório de cobertura

```bash
dotnet test --settings coverlet.runsettings
```

Os XMLs são gerados em `tests/**/TestResults/**/coverage.opencover.xml`.

---

## Qualidade — SonarQube

O SonarQube fica separado do compose principal (imagem pesada ~1 GB).

```bash
# Subir SonarQube
cd sonar && docker compose up -d

# Aguarde ~2 min e acesse http://localhost:9000 (admin/admin)
# Gere um token em: My Account → Security → Generate Token

# Da raiz do projeto:
dotnet sonarscanner begin /k:"tech-challenge" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="SEU_TOKEN" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" /d:sonar.coverage.exclusions="**/Program.cs,**/*Tests.cs,**/Migrations/**"
dotnet build
dotnet test --no-build --settings coverlet.runsettings
dotnet sonarscanner end /d:sonar.token="SEU_TOKEN"
```

---

## Desenvolvimento local (sem Docker)

Para rodar sem Docker, configure o PostgreSQL local e defina a connection string no `appsettings.json` ou via user-secrets. O RabbitMQ pode ficar desligado — basta manter `RabbitMq:Enabled=false` (padrão) nos `appsettings.json`, o que ativa o stub de log.

```bash
dotnet run --project src/Atendimento/OficinaMecanica.Atendimento.API
dotnet run --project src/Estoque/OficinaMecanica.Estoque.API
```

---

## Licença

Projeto acadêmico — FIAP Pós-graduação em Software Architecture. Licença MIT.
