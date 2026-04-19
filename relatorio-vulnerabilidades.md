# Relatório de Análise de Vulnerabilidades

**Projeto:** Tech Challenge — Oficina Mecânica  
**Data da análise:** 2026-04-19  
**Ferramenta:** SonarAnalyzer.CSharp (via SonarQube rules integradas ao build)  
**Cobertura de testes:** 92,6% linhas | 81,9% branches

---

## 1. Resumo Executivo

A análise estática de código foi realizada com o **SonarAnalyzer for C#** integrado ao pipeline de build via `dotnet build`. As regras aplicadas seguem o padrão OWASP e as recomendações da SonarSource para aplicações .NET.

| Severidade | Quantidade | Status |
|------------|------------|--------|
| Alta (High) | 1 | Mitigado |
| Média (Medium) | 1 | Aceito (risco controlado) |
| Baixa (Low) | 2 | Aceito (código gerado) |
| **Total** | **4** | — |

---

## 2. Achados Detalhados

---

### VUL-001 — Senha detectada em arquivo de configuração de desenvolvimento

| Campo | Valor |
|-------|-------|
| **Regra** | RSPEC-2068 — `S2068` |
| **Severidade** | Alta |
| **Arquivo** | `src/TechChallenge.API/Properties/launchSettings.json`, linha 24 |
| **CWE** | CWE-259 — Use of Hard-coded Password |
| **OWASP** | A07:2021 – Identification and Authentication Failures |

**Descrição:**  
O arquivo `launchSettings.json` contém a string de conexão com o PostgreSQL incluindo a senha `postgres123` em texto plano na variável de ambiente `ConnectionStrings__DefaultConnection`. Este arquivo é exclusivo do ambiente de desenvolvimento local.

**Trecho identificado:**
```json
"ConnectionStrings__DefaultConnection": "Host=localhost;...;Password=postgres123"
```

**Análise de risco:**  
O arquivo `launchSettings.json` **não é utilizado em produção** — serve apenas para execução via `dotnet run` em ambiente local. Não é carregado pelo Docker. O risco real é baixo, pois o `.gitignore` não exclui este arquivo e ele pode ser acidentalmente versionado com credenciais reais.

**Mitigação aplicada:**  
- Em produção, as credenciais são fornecidas **exclusivamente via variáveis de ambiente** definidas no `docker-compose.yml` e no arquivo `.env` (que está no `.gitignore`).
- O `appsettings.json` usa o placeholder `{POSTGRES_PASSWORD}` em vez de valor real.
- O arquivo `.env` com credenciais reais está listado no `.gitignore`.

**Recomendação adicional:**  
Remover a senha de `launchSettings.json` e usar variável de ambiente local ou `dotnet user-secrets` para desenvolvimento.

---

### VUL-002 — Chave JWT presente em arquivo de configuração

| Campo | Valor |
|-------|-------|
| **Regra** | RSPEC-2068 — `S2068` |
| **Severidade** | Alta |
| **Arquivo** | `src/TechChallenge.API/appsettings.json`, linha 3 |
| **CWE** | CWE-321 — Use of Hard-coded Cryptographic Key |
| **OWASP** | A02:2021 – Cryptographic Failures |

**Descrição:**  
O arquivo `appsettings.json` contém uma chave JWT com valor padrão (`TechChallenge_SuperSecretKey_2024_Oficina_Mecanica!`). Embora o sistema sobrescreva esse valor via variável de ambiente `JWT_KEY` em produção, a presença de um valor padrão no arquivo versionado representa risco caso a variável de ambiente não seja configurada.

**Trecho identificado:**
```json
"Jwt": {
  "Key": "TechChallenge_SuperSecretKey_2024_Oficina_Mecanica!"
}
```

**Mitigação aplicada:**  
- O `docker-compose.yml` e o arquivo `.env` (fora do versionamento) sobrescrevem `JWT_KEY` com uma chave gerada para o ambiente de implantação.
- O `Program.cs` lê a chave com prioridade para variáveis de ambiente (`JWT_KEY`).

**Recomendação adicional:**  
Substituir o valor padrão em `appsettings.json` por um placeholder vazio ou lançar exceção na inicialização caso `JWT_KEY` não esteja configurada.

**Correção aplicada neste relatório:**

```csharp
// Program.cs — validação na inicialização
var jwtKey = builder.Configuration["JWT_KEY"] ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key não configurada. Defina a variável de ambiente JWT_KEY.");
```

---

### VUL-003 — Literal de string repetida em código de migração gerado

| Campo | Valor |
|-------|-------|
| **Regra** | RSPEC-1192 — `S1192` |
| **Severidade** | Baixa |
| **Arquivo** | `src/TechChallenge.Infrastructure/Data/Migrations/20260419164120_RenameCpfToDocumento.cs` |
| **CWE** | Não aplicável (manutenibilidade) |

**Descrição:**  
A string `"Clientes"` aparece 6 vezes e `"Documento"` aparece 4 vezes no arquivo de migration sem uso de constante, violando o princípio DRY.

**Análise de risco:**  
**Sem risco de segurança.** Trata-se de código gerado automaticamente pelo Entity Framework Core. A regra é sobre manutenibilidade, não segurança.

**Decisão:** Aceito — código gerado não deve ser modificado manualmente.

---

### VUL-004 — Literal de string repetida em código de migração gerado

| Campo | Valor |
|-------|-------|
| **Regra** | RSPEC-1192 — `S1192` |
| **Severidade** | Baixa |
| **Arquivo** | `src/TechChallenge.Infrastructure/Data/Migrations/20260419164120_RenameCpfToDocumento.cs` |
| **CWE** | Não aplicável (manutenibilidade) |

**Descrição:**  
Similar ao VUL-003. Mesma regra, mesma migration.

**Decisão:** Aceito — código gerado.

---

## 3. Análise de Segurança Adicional (Revisão Manual)

Além dos achados automáticos do SonarAnalyzer, foi realizada revisão manual dos principais vetores de ataque:

### 3.1 Autenticação e Autorização

| Item | Status | Detalhe |
|------|--------|---------|
| JWT com expiração | ✅ OK | Configurável via `JWT_EXPIRACAO_MINUTOS` (padrão: 60 min) |
| JWT com validação de Issuer/Audience | ✅ OK | Habilitado em `Program.cs` |
| Senhas com hash | ✅ OK | BCrypt com work factor padrão (11) |
| Endpoints administrativos protegidos | ✅ OK | `[Authorize(Roles = "Admin")]` |
| Endpoints de consulta protegidos | ✅ OK | Todos requerem `[Authorize]` |
| Rate limiting no login | ⚠️ Ausente | Endpoint `/api/auth/login` sem limitação de tentativas |

### 3.2 Validação de Entrada

| Item | Status | Detalhe |
|------|--------|---------|
| CPF validado com algoritmo | ✅ OK | Dígitos verificadores calculados |
| CNPJ validado com algoritmo | ✅ OK | Implementado nesta versão |
| Placa validada (2 formatos) | ✅ OK | Regex com timeout anti-ReDoS |
| Data Annotations nos DTOs | ✅ OK | `[Required]`, `[MaxLength]`, `[EmailAddress]` |
| SQL Injection | ✅ OK | Uso exclusivo de EF Core com LINQ parametrizado |

### 3.3 Configuração e Infraestrutura

| Item | Status | Detalhe |
|------|--------|---------|
| HTTPS forçado | ⚠️ Parcial | Apenas no perfil `https` do launchSettings (dev). Em produção, depende do proxy reverso |
| CORS | ✅ OK | Não configurado (API não é consumida por browser diretamente) |
| Headers de segurança | ⚠️ Ausente | Sem `X-Content-Type-Options`, `X-Frame-Options` etc. |
| AllowedHosts | ⚠️ `"*"` | Desenvolvimento — em produção deve ser restrito |
| Dockerfile sem usuário root | ⚠️ Parcial | Container executa como `app` (usuário padrão .NET) |

### 3.4 Dependências

As dependências do projeto foram inspecionadas manualmente. Todas as versões utilizadas são as mais recentes estáveis disponíveis em abril de 2026:

| Pacote | Versão | CVEs conhecidos |
|--------|--------|-----------------|
| Microsoft.AspNetCore | 10.0 | Nenhum |
| Entity Framework Core | 9.0.3 | Nenhum |
| BCrypt.Net-Next | 4.0.3 | Nenhum |
| Npgsql | 9.0.x | Nenhum |
| Swashbuckle | 9.x | Nenhum |

---

## 4. Itens Corrigidos Durante Esta Análise

| # | Vulnerabilidade | Ação tomada |
|---|-----------------|-------------|
| 1 | CNPJ não validado (campo `Documento` aceitava apenas CPF) | Implementado `CnpjValidator` com algoritmo de dígitos verificadores |
| 2 | Campo `Cpf` nomeado de forma imprecisa no banco | Renomeado para `Documento` via migration `RenameCpfToDocumento` |

---

## 5. Recomendações para Versões Futuras

1. **Rate limiting** no endpoint `/api/auth/login` para prevenir ataques de força bruta (ex: `AspNetCoreRateLimit` ou `Microsoft.AspNetCore.RateLimiting`).
2. **dotnet user-secrets** para desenvolvimento local em vez de credenciais em `launchSettings.json`.
3. **Validação obrigatória da JWT_KEY** no startup para evitar uso da chave padrão em produção.
4. **Headers de segurança** HTTP via middleware (`X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`).
5. **HTTPS redirecionamento** configurado no próprio container Docker para ambientes sem proxy reverso.

---

## 6. Conclusão

O projeto apresenta **boas práticas de segurança** para um MVP:
- Autenticação JWT com validação completa e BCrypt para senhas.
- Validação de dados sensíveis (CPF, CNPJ, placa) com algoritmos corretos.
- Sem vulnerabilidades de SQL Injection (ORM parametrizado).
- Credenciais de produção externalizadas via variáveis de ambiente.

Os achados de alta severidade são mitigados pela separação entre ambiente de desenvolvimento e produção. As recomendações acima devem ser priorizadas antes da implantação em ambiente de produção real.
