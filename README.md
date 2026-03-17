# Tech Challenge - Oficina Mecânica

API REST para gerenciamento de oficina mecânica, desenvolvida com arquitetura em camadas (DDD - Domain-Driven Design), utilizando .NET 10.0, Entity Framework Core e PostgreSQL.

## Tecnologias Utilizadas

- **Backend**: .NET 10.0 (ASP.NET Core Web API)
- **Banco de Dados**: PostgreSQL
- **ORM**: Entity Framework Core com Npgsql
- **Autenticação**: JWT (JSON Web Tokens)
- **Documentação**: Swagger/OpenAPI
- **Containerização**: Docker & Docker Compose
- **Arquitetura**: DDD (Domain-Driven Design) com camadas separadas (Domain, Application, Infrastructure, API)

## Estrutura do Projeto

```
src/
├── TechChallenge.API/          # Camada de apresentação (Controllers, Program.cs)
├── TechChallenge.Application/  # Camada de aplicação (Services, DTOs, Interfaces)
├── TechChallenge.Domain/       # Camada de domínio (Entities, Enums, Interfaces de repositório)
└── TechChallenge.Infrastructure/ # Camada de infraestrutura (Repositories, DbContext, Migrations)
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
   dotnet run --project TechChallenge.API
   ```

5. **Acesse**:
   - API: http://localhost:5121 (porta padrão do launchSettings.json)
   - Swagger: http://localhost:5121/swagger

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

- **Admin**: admin@oficina.com / Admin@123
- **Mecânico**: mecanico@oficina.com / Mec@123
- **Atendente**: atendente@oficina.com / Ate@123

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
- `PUT /api/ordensservico/{id}` - Atualizar ordem
- `DELETE /api/ordensservico/{id}` - Cancelar ordem

## Desenvolvimento

### Executar Testes
```bash
dotnet test
```

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

Este projeto é parte do Tech Challenge e está sob licença MIT.