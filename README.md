# Tech Challenge — Oficina Mecânica

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13-239120?logo=csharp)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3-FF6600?logo=rabbitmq&logoColor=white)
![Kubernetes](https://img.shields.io/badge/Kubernetes-1.31-326CE5?logo=kubernetes&logoColor=white)
![Terraform](https://img.shields.io/badge/Terraform-1.6+-844FBA?logo=terraform&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI%2FCD-2088FF?logo=githubactions&logoColor=white)
![SonarCloud](https://img.shields.io/badge/SonarCloud-análise-F3702A?logo=sonarcloud&logoColor=white)

Sistema de gerenciamento de oficina mecânica construído com arquitetura de **microsserviços hexagonal**, comunicação assíncrona via **RabbitMQ/MassTransit** e persistência em **PostgreSQL**.

Projeto acadêmico da pós-graduação em Software Architecture — FIAP.

---

## Descrição da solução

O sistema gerencia o ciclo completo de uma ordem de serviço: abertura, execução, aprovação de orçamento, finalização e baixa de estoque. É dividido em dois microsserviços independentes que se comunicam via mensageria:

- **Atendimento**: clientes, veículos, catálogo de serviços, ordens de serviço e autenticação JWT
- **Estoque**: cadastro de peças, movimentações e baixa automática ao finalizar uma OS

**Objetivos desta fase:** escalabilidade (HPA), resiliência (circuit breaker, retry, dead letter), observabilidade (Prometheus, Grafana, Loki, Jaeger) e CI/CD completo com GitHub Actions.

Decisões arquiteturais registradas em [`docs/adr/`](docs/adr/).

---

## Arquitetura

![Visão geral](docs/images/img-metricas.png)

### Componentes e comunicação entre serviços

![Arquitetura — componentes e comunicação](docs/images/img-arquitetura.png)

Dois microserviços independentes com comunicação **síncrona HTTP** apenas na abertura de OS (verificação de disponibilidade de peças via Polly) e **assíncrona via RabbitMQ** na finalização (evento `os.finalizada` → baixa de estoque).

Em caso de falha no consumer, o MassTransit faz **retry automático** (3 tentativas: 1s → 5s → 10s). Após esgotar, a mensagem vai para a queue `estoque.baixa_error`.

### Arquitetura Hexagonal — camadas por serviço

![Arquitetura Hexagonal](docs/images/img-hexagonal.png)

Regra de ouro: `Domain` sem nenhum `using` externo · `Application` não referencia `Infrastructure`.

### Infraestrutura Kubernetes

![Infraestrutura Kubernetes](docs/images/img-kubernetes.png)

### Fluxo de deploy — CI/CD

![Pipeline CI/CD](docs/images/img-cicd.png)

```
build-and-test (ubuntu-latest)
       ↓
    infra (self-hosted)   ← terraform init + apply provisiona cluster Kind e PostgreSQL
       ↓
    docker (ubuntu-latest) ← build e push das imagens para GHCR
       ↓
    deploy (self-hosted)  ← kubectl apply dos manifestos K8s
```

Jobs em sequência obrigatória via `needs:` · Terraform orquestrado pelo pipeline (não manual) · imagens fixadas por hash de commit (supply chain) · self-hosted runner no cluster local · `infra` e `deploy` rodam apenas em push para `main`.

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
| Orquestração | Kubernetes 1.31 (Kind local) |
| Infra como código | Terraform >= 1.6 |
| Testes | xUnit + Moq + FluentAssertions + Testcontainers |
| Cobertura | coverlet (OpenCover) |
| Qualidade | SonarAnalyzer for C# + SonarCloud |
| Observabilidade | Prometheus + Grafana + Loki + Promtail + Jaeger |

---

## Estrutura do projeto

```
src/
├── Atendimento/
│   ├── OficinaMecanica.Atendimento.Domain/          # Entidades, enums, regras de negócio
│   ├── OficinaMecanica.Atendimento.Application/     # Use cases, ports (interfaces), eventos
│   ├── OficinaMecanica.Atendimento.Infrastructure/  # EF Core, repositórios, RabbitMQ publisher
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

k8s/
├── namespace.yaml
├── atendimento/     # deployment, service, configmap, secret, hpa
├── estoque/         # deployment, service, configmap, secret, hpa
├── postgres/        # deployment, service, secret, pvc
├── rabbitmq/        # statefulset, service, pvc
└── observabilidade/ # prometheus, grafana, loki, promtail, jaeger

infra/
├── main.tf          # raiz: chama módulos cluster e database
├── variables.tf     # cluster_name, kubernetes_version, db_*
├── outputs.tf       # cluster_endpoint, postgres_connection_string
├── versions.tf      # providers: kind, kubernetes, docker, null
└── modules/
    ├── cluster/     # kind_cluster + namespaces oficina-mecanica e observabilidade
    └── database/    # docker_container postgres na rede kind + schemas

docs/
├── adr/             # Architecture Decision Records
└── cards/           # Cartões de implementação por sprint
```

---

## Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para rodar tudo com compose)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (para desenvolvimento local e testes)

---

## Como executar localmente

### 1. Configure o arquivo `.env`

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

## Deploy em Kubernetes

### Pré-requisitos

- `kubectl` configurado apontando para o cluster
- Imagens publicadas no GHCR via CI/CD (ou substituir pelas suas)

### Passo a passo

```bash
# 1. Criar namespace
kubectl apply -f k8s/namespace.yaml

# 2. Criar secrets (substituir pelos valores reais)
kubectl create secret generic atendimento-secrets \
  --from-literal=Jwt__Key=<chave-minimo-32-chars> \
  --from-literal=ConnectionStrings__DefaultConnection="Host=postgres-svc;Port=5432;Database=oficina_atendimento;Username=postgres;Password=<senha>" \
  -n oficina-mecanica

kubectl create secret generic estoque-secrets \
  --from-literal=ConnectionStrings__DefaultConnection="Host=postgres-svc;Port=5432;Database=oficina_estoque;Username=postgres;Password=<senha>" \
  -n oficina-mecanica

# 3. Aplicar manifestos
kubectl apply -f k8s/ -n oficina-mecanica
kubectl apply -f k8s/observabilidade/ -n observabilidade

# 4. Acompanhar rollout
kubectl rollout status deployment/atendimento -n oficina-mecanica
kubectl rollout status deployment/estoque -n oficina-mecanica

# 5. Verificar HPA
kubectl get hpa -n oficina-mecanica
```

> **Nota:** o startup da API cria o banco automaticamente se não existir e roda as migrations do EF Core.

---

## Provisionamento com Terraform

O Terraform provisiona o cluster Kind local e o container PostgreSQL, conectando-o à rede Docker do Kind para que os pods consigam acessá-lo.

### Estrutura dos módulos

| Módulo | O que provisiona |
|---|---|
| `modules/cluster` | Cluster Kind com `kindest/node:v1.31.0`, expõe portas 30080/30081/30090 no host, cria namespaces `oficina-mecanica` e `observabilidade` |
| `modules/database` | Container `postgres:16` na rede `kind`, cria schemas `atendimento` e `estoque` via `local-exec` |

### Providers utilizados

| Provider | Versão | Finalidade |
|---|---|---|
| `tehcyx/kind` | ~> 0.4 | Criar e configurar o cluster Kind |
| `hashicorp/kubernetes` | ~> 2.31 | Criar namespaces no cluster |
| `kreuzwerker/docker` | ~> 3.0 | Gerenciar o container PostgreSQL |
| `hashicorp/null` | ~> 3.2 | Executar scripts locais pós-provisionamento |

### Variáveis

| Variável | Default | Descrição |
|---|---|---|
| `cluster_name` | `oficina-mecanica` | Nome do cluster Kind |
| `kubernetes_version` | `v1.31.0` | Versão da imagem do nó |
| `db_name` | `oficinamecanica` | Nome do banco PostgreSQL |
| `db_user` | `oficina` | Usuário do PostgreSQL |
| `db_password` | _(obrigatório)_ | Senha do PostgreSQL |
| `db_port` | `5433` | Porta exposta no host |

### Executar

```bash
cd infra
terraform init
terraform plan -var="db_password=suasenha"
terraform apply -var="db_password=suasenha"

# Outputs após o apply:
# cluster_name                  = "oficina-mecanica"
# cluster_endpoint              = "https://127.0.0.1:..."
# postgres_connection_string    = <sensitive>
# postgres_host_connection_string = <sensitive>

# Ver valores sensitive:
terraform output -raw postgres_host_connection_string
```

---

## APIs — Swagger e Postman

A collection completa do Postman com todos os endpoints está em [`docs/oficina-mecanica.postman_collection.json`](docs/oficina-mecanica.postman_collection.json). Importe no Postman e configure a variável `baseUrl` para `http://localhost:8080`.

Após subir a aplicação, o Swagger também está disponível:

| Serviço | Swagger UI |
|---|---|
| Atendimento | http://localhost:8080/swagger |
| Estoque | http://localhost:8081/swagger |

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

### Com relatório de cobertura

```bash
dotnet test --settings coverlet.runsettings
```

Os XMLs são gerados em `tests/**/TestResults/**/coverage.opencover.xml`.

---

## Observabilidade

### Logs dos containers

```bash
docker compose logs -f
docker compose logs -f api           # Atendimento
docker compose logs -f estoque-api   # Estoque
```

### RabbitMQ Management UI

Acesse http://localhost:15672 com `guest` / `guest` para visualizar:

- **Queues** → `estoque.baixa` — mensagens pendentes
- **Queues** → `estoque.baixa_error` — dead letter (falhas após 3 retries)
- **Exchanges** → `OficinaMecanica.Estoque.Application.Events:OsFinalizadaEvent`

---

## Qualidade — SonarCloud

A análise roda automaticamente no CI a cada push em `main`. Para rodar localmente:

```bash
dotnet sonarscanner begin /k:"tech-challenge" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.token="SEU_TOKEN" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"
dotnet build
dotnet test --no-build --settings coverlet.runsettings
dotnet sonarscanner end /d:sonar.token="SEU_TOKEN"
```

---

## Vídeo demonstrativo

> Link a ser adicionado após gravação (YouTube ou Vimeo, até 15 minutos).

---

## Licença

Projeto acadêmico — FIAP Pós-graduação em Software Architecture. Licença MIT.
