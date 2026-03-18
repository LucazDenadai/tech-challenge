# Tech Challenge - Oficina Mecânica

API REST para gerenciamento de oficina mecânica, desenvolvida com arquitetura em camadas (DDD - Domain-Driven Design), utilizando .NET 10.0, Entity Framework Core e PostgreSQL.

## Tecnologias Utilizadas

- **Backend**: .NET 10.0 (ASP.NET Core Web API)
- **Banco de Dados**: PostgreSQL
- **ORM**: Entity Framework Core com Npgsql
- **Autenticação**: JWT (JSON Web Tokens)
- **Documentação**: Swagger/OpenAPI
- **Containerização**: Docker & Docker Compose
- **Testes**: xUnit, Moq, FluentAssertions, Testcontainers
- **Cobertura**: coverlet (formato OpenCover)
- **Lint**: .editorconfig + `dotnet format`
- **Arquitetura**: DDD (Domain-Driven Design) com camadas separadas

## Estrutura do Projeto

```
src/
├── TechChallenge.API/             # Camada de apresentação (Controllers, Program.cs)
├── TechChallenge.Application/     # Camada de aplicação (Services, DTOs, Interfaces)
├── TechChallenge.Domain/          # Camada de domínio (Entities, Enums, Interfaces de repositório)
└── TechChallenge.Infrastructure/  # Camada de infraestrutura (Repositories, DbContext, Migrations)

tests/
├── TechChallenge.UnitTests/       # Testes unitários (domínio e serviços de aplicação)
└── TechChallenge.IntegrationTests/ # Testes de integração E2E (HTTP + PostgreSQL real)
```

## Pré-requisitos

- .NET 10.0 SDK
- Docker & Docker Compose
- PostgreSQL (opcional, se não usar Docker)

## Como Executar

### Com Docker (Recomendado)

1. **Clone o repositório**:
   ```bash
   git clone https://github.com/LucazDenadai/Tech-challenge.git
   cd Tech-challenge
   ```

2. **Execute o Docker Compose**:
   ```bash
   docker-compose up --build
   ```

3. **Acesse a aplicação**:
   - API: http://localhost:8080
   - Swagger: http://localhost:8080/swagger

> **Nota sobre SonarQube**: O `docker-compose.yml` inclui o SonarQube (http://localhost:9000), mas ele **não é iniciado por padrão** porque seu download é pesado (~1 GB de imagens). Para habilitá-lo, veja a seção [Análise de Qualidade com SonarQube](#análise-de-qualidade-com-sonarqube) abaixo.

### Sem Docker (Desenvolvimento Local)

1. **Configure o PostgreSQL**:
   - Instale e execute PostgreSQL localmente.
   - Crie um banco chamado `techchallengedb` com usuário `postgres` e senha `postgres123`.

2. **Restaure os pacotes**:
   ```bash
   dotnet restore
   ```

3. **Execute as migrations** (se necessário):
   ```bash
   cd src/TechChallenge.API
   dotnet ef database update
   ```

4. **Execute a aplicação**:
   ```bash
   dotnet run --project src/TechChallenge.API
   ```

5. **Acesse**:
   - API: http://localhost:5121 (porta padrão do launchSettings.json)
   - Swagger: http://localhost:5121/swagger

## Testes

### Testes Unitários

Testam a lógica de negócio isolada (domínio e serviços de aplicação) **sem dependência de banco de dados**.

```bash
dotnet test tests/TechChallenge.UnitTests/
```

Cobertura:
- **Domínio**: `OrdemServico` (status, ValorTotal), `Peca` (estoque), `Cliente`
- **Services**: `ClienteService`, `PecaService`, `OrdemServicoService`, `AuthService`

### Testes de Integração

Testam o fluxo completo HTTP → Controller → Service → Repository → **PostgreSQL real** via [Testcontainers](https://testcontainers.com/).

> **Requer Docker instalado e em execução.** O container PostgreSQL é iniciado e destruído automaticamente.

```bash
dotnet test tests/TechChallenge.IntegrationTests/
```

Cobertura:
- **Auth**: login válido, senha errada, usuário inexistente, perfis
- **Clientes**: CRUD completo, CPF duplicado, 401 sem auth, 404
- **Peças**: CRUD completo, listagem seeded

### Todos os testes

```bash
dotnet test
```

### Com relatório de cobertura

O arquivo `coverlet.runsettings` gera o formato **OpenCover** (necessário para o SonarQube):

```bash
dotnet test --settings coverlet.runsettings
```

Os XMLs são gerados em `tests/**/TestResults/**/coverage.opencover.xml`. Para visualização HTML local, use [ReportGenerator](https://reportgenerator.io/):

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.opencover.xml" -targetdir:"coverage-report" -reporttypes:Html
open coverage-report/index.html
```

## Autenticação

A API utiliza JWT para autenticação. Para acessar endpoints protegidos:

1. **Faça login** via POST `/api/auth/login`:
   ```json
   {
     "email": "admin@oficina.com",
     "senha": "Admin@123"
   }
   ```

2. **Use o token retornado** no header `Authorization: Bearer {token}`.

### Usuários de Teste (Seeded)

| Perfil     | Email                      | Senha       |
|------------|---------------------------|-------------|
| Admin      | admin@oficina.com         | Admin@123   |
| Mecânico   | mecanico@oficina.com      | Mec@123     |
| Atendente  | atendente@oficina.com     | Ate@123     |

## Endpoints Principais

### Autenticação
- `POST /api/auth/login` - Login e obtenção de token JWT

### Clientes
- `GET /api/clientes` - Listar todos os clientes
- `GET /api/clientes/{id}` - Obter cliente por ID
- `POST /api/clientes` - Criar novo cliente
- `PUT /api/clientes/{id}` - Atualizar cliente
- `DELETE /api/clientes/{id}` - Desativar cliente

### Veículos
- `GET /api/veiculos` - Listar todos os veículos
- `GET /api/veiculos/{id}` - Obter veículo por ID
- `POST /api/veiculos` - Criar novo veículo
- `PUT /api/veiculos/{id}` - Atualizar veículo
- `DELETE /api/veiculos/{id}` - Desativar veículo

### Serviços
- `GET /api/servicos` - Listar todos os serviços
- `GET /api/servicos/{id}` - Obter serviço por ID
- `POST /api/servicos` - Criar novo serviço
- `PUT /api/servicos/{id}` - Atualizar serviço
- `DELETE /api/servicos/{id}` - Desativar serviço

### Peças
- `GET /api/pecas` - Listar todas as peças
- `GET /api/pecas/{id}` - Obter peça por ID
- `POST /api/pecas` - Criar nova peça
- `PUT /api/pecas/{id}` - Atualizar peça
- `DELETE /api/pecas/{id}` - Desativar peça

### Ordens de Serviço
- `GET /api/ordensservico` - Listar todas as ordens
- `GET /api/ordensservico/{id}` - Obter ordem por ID
- `POST /api/ordensservico` - Criar nova ordem
- `PUT /api/ordensservico/{id}/status` - Avançar status
- `POST /api/ordensservico/{id}/servicos` - Adicionar item de serviço
- `POST /api/ordensservico/{id}/pecas` - Adicionar item de peça

## Lint

O projeto usa `.editorconfig` como fonte de regras de estilo. Para verificar ou corrigir formatação localmente:

```bash
# Verificar sem alterar (modo CI)
dotnet format --verify-no-changes

# Corrigir automaticamente
dotnet format
```

## Análise de Qualidade com SonarQube

O SonarQube **fica separado do docker-compose principal** pelo tamanho do download (~1 GB de imagens Docker).

### Subindo o SonarQube

```bash
cd sonar
docker compose up -d
```

Aguarde 1-2 minutos e acesse http://localhost:9000 (admin/admin na primeira vez).

### Executando a análise com cobertura

> **Importante**: o `dotnet test` com cobertura deve rodar **entre** o `begin` e o `end` do sonarscanner para que o Sonar processe os relatórios corretamente.

> **PowerShell**: use `` ` `` (backtick) para quebrar linhas. Os comandos abaixo estão em uma linha só para facilitar o copy-paste.

```powershell
# 1. Instale o sonarscanner (uma vez)
dotnet tool install -g dotnet-sonarscanner

# 2. Inicia a sessão de análise
dotnet sonarscanner begin /k:"tech-challenge" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="admin" /d:sonar.password="admin" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" /d:sonar.coverage.exclusions="**/Program.cs,**/*Tests.cs,**/Migrations/**"

# 3. Build
dotnet build

# 4. Testes com cobertura (gera os XMLs que o Sonar vai ler)
dotnet test --settings coverlet.runsettings

# 5. Finaliza e envia os resultados
dotnet sonarscanner end /d:sonar.login="admin" /d:sonar.password="admin"
```

Visualize os resultados em http://localhost:9000/projects.

## Desenvolvimento

### Build
```bash
dotnet build
```

### Migrations
```bash
cd src/TechChallenge.API
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

## Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/nova-feature`)
3. Commit suas mudanças (`git commit -am 'Adiciona nova feature'`)
4. Push para a branch (`git push origin feature/nova-feature`)
5. Abra um Pull Request

## Licença

Este projeto é parte do Tech Challenge da FIAP e está sob licença MIT.
