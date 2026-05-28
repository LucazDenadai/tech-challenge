# Experiências Técnicas — Tech Challenge Oficina Mecânica

> Documento de registro de problemas reais enfrentados durante o desenvolvimento, escritos no método **STAR** (Situação, Tarefa, Ação, Resultado) para facilitar explicação em entrevistas, apresentações e conversas técnicas.

---

## EXP-001 — Concorrência na reserva de peças com EF Core

### Situação

Durante a fase anterior do projeto, o sistema permitia que duas requisições simultâneas de abertura de OS verificassem a disponibilidade de uma peça e ambas obtivessem `disponível = true` — mesmo que houvesse apenas uma unidade em estoque. A segunda OS era criada com sucesso, mas no momento da baixa física a quantidade ficava negativa. O problema era silencioso: nenhuma exceção era lançada, o banco aceitava o valor negativo.

### Tarefa

Garantir que a verificação de disponibilidade e a reserva de peça fossem atômicas — se dois pedidos chegassem ao mesmo tempo para a última unidade, apenas um deveria ser aceito.

### Ação

Identificou-se que o EF Core, por padrão, não aplica lock nas leituras — dois `SELECT` simultâneos retornam o mesmo snapshot sem se bloquearem. A solução foi usar **concorrência otimista com row versioning**:

1. Adicionou-se um campo `RowVersion` (tipo `byte[]` com `[Timestamp]`) na entidade `Peca`
2. O EF Core configurou automaticamente o `WHERE "RowVersion" = @original` no `UPDATE`
3. Se duas transações tentassem salvar simultaneamente, a segunda receberia `DbUpdateConcurrencyException`
4. O use case captura essa exceção e retorna HTTP 409 (Conflict) com mensagem clara para o cliente

```csharp
// Entidade
[Timestamp]
public byte[] RowVersion { get; set; }

// Use case
catch (DbUpdateConcurrencyException)
{
    throw new ConflictException("Peça foi modificada por outra operação. Tente novamente.");
}
```

A alternativa considerada foi pessimistic locking (`SELECT FOR UPDATE` via SQL raw), mas foi rejeitada por degradar o throughput em toda leitura, não apenas nas contendidas.

### Resultado

O problema de saldo negativo foi eliminado. Em carga paralela com 10 requisições simultâneas para a última unidade, apenas uma era aprovada e as demais recebiam 409. O comportamento ficou correto sem impacto de performance nas operações normais, pois a exceção só ocorre em contenda real.

**Conceito transmissível:** concorrência otimista é a escolha certa quando a contenda é baixa e o custo de releitura é aceitável. Pessimistic locking só vale quando a contenda é alta e você quer evitar retentativas.

---

## EXP-002 — Mensagem descartada silenciosamente no RabbitMQ (namespace mismatch)

### Situação

Após implementar o fluxo de baixa automática de estoque via RabbitMQ (CARD-09), o consumer do microserviço Estoque simplesmente não executava. A OS era finalizada com sucesso no Atendimento, o evento era publicado, o RabbitMQ recebia a mensagem — mas o estoque não era reduzido. Nenhum erro aparecia nos logs. A fila `estoque.baixa` acumulava mensagens, mas elas sumiam sem ser processadas.

### Tarefa

Descobrir por que o consumer `BaixaEstoqueConsumer` não era invocado mesmo com a mensagem chegando na fila, e corrigir sem alterar o contrato público do evento.

### Ação

A investigação começou pela Management UI do RabbitMQ (`localhost:15672`). Lá foi encontrada uma fila chamada **`estoque.baixa_skipped`** com todas as mensagens descartadas. O nome `_skipped` é o sufixo que o MassTransit usa quando recebe uma mensagem mas **não encontra nenhum consumer registrado para aquele tipo**.

Inspecionando o header da mensagem via RabbitMQ UI, o campo `messageType` era:

```
urn:message:OficinaMecanica.Atendimento.Application.Events:OsFinalizadaEvent
```

O consumer estava registrado para:

```
urn:message:OficinaMecanica.Estoque.Application.Events:OsFinalizadaEvent
```

**Root cause:** o MassTransit não usa o nome da classe como identificador — usa o **namespace C# completo + nome da classe** como `messageType`. O Estoque tinha uma cópia do arquivo `OsFinalizadaEvent.cs` com namespace próprio (`OficinaMecanica.Estoque.Application.Events`), diferente do namespace do Atendimento. Para o MassTransit, eram dois tipos completamente distintos.

A correção foi alterar o namespace da cópia no Estoque para ser idêntico ao do Atendimento:

```csharp
// Antes (Estoque)
namespace OficinaMecanica.Estoque.Application.Events;

// Depois — namespace idêntico ao Atendimento para o MassTransit reconhecer o messageType
namespace OficinaMecanica.Atendimento.Application.Events;
```

O comentário foi mantido no arquivo para documentar que aquele namespace é intencional e não deve ser "corrigido" por um desenvolvedor no futuro.

### Resultado

Após o alinhamento de namespace, as mensagens pararam de ir para `_skipped` e o consumer passou a executar. O estoque foi reduzido corretamente ao finalizar a OS (10 unidades → 8 após uma OS com 2 peças). A fila `estoque.baixa_skipped` ficou vazia e a tabela `FalhasProcessamento` permaneceu sem registros — comportamento esperado em operação saudável.

**Conceito transmissível:** em sistemas de mensageria com MassTransit, o contrato da mensagem é o **namespace + nome da classe**, não apenas o nome. Em arquiteturas de microsserviços, o arquivo de contrato (o `record` ou `class` do evento) precisa ter namespace idêntico nos dois lados, ou usar uma biblioteca compartilhada de contratos. A fila `_skipped` é o sinal diagnóstico chave: se mensagens vão para `_skipped`, o problema é de tipo/namespace, não de lógica de negócio.

---

## EXP-003 — EF Core bloqueando startup por modelo divergente da migration

### Situação

Após a decisão arquitetural de remover o catálogo de peças do microserviço Atendimento (ADR-004), a entidade `Peca` e sua FK em `ItensPeca` foram deletadas do código. Na primeira execução após a remoção, o serviço lançava um aviso `PendingModelChangesWarning` durante o `MigrateAsync()` no startup — em modo Release, esse aviso era tratado como erro e impedia o serviço de iniciar.

### Tarefa

Sincronizar o modelo do EF Core com o estado atual do código, garantindo que a migration cubra exatamente as mudanças feitas (drop da tabela `Pecas` e da FK) sem gerar migration com mudanças extras.

### Ação

O EF Core compara o modelo em memória (derivado das entidades C# registradas no `DbContext`) com o snapshot salvo na última migration. Ao remover a entidade `Peca` do `DbContext`, o modelo em memória ficou diferente do snapshot — o EF detectou a divergência.

A sequência correta foi:

```bash
# 1. Build obrigatório antes — EF lê o assembly compilado, não o código-fonte
dotnet build

# 2. Criar a migration que captura o diff
dotnet ef migrations add RemoveCatalogoPecasAtendimento \
  --project src/Atendimento/OficinaMecanica.Atendimento.Infrastructure \
  --startup-project src/Atendimento/OficinaMecanica.Atendimento.API

# 3. Verificar o conteúdo gerado antes de commitar
# A migration deve conter: DropForeignKey, DropTable("Pecas"), DropIndex
```

A migration gerada continha exatamente:
- `DropForeignKey("FK_ItensPeca_Pecas_PecaId")`
- `DropTable("Pecas")`
- `DropIndex("IX_ItensPeca_PecaId")`

Um erro comum nessa situação é rodar `dotnet ef` sem buildar antes — o EF lê o último assembly compilado, então leria o código antigo ainda com `Peca` e geraria uma migration vazia ou incorreta.

### Resultado

Após a migration, o startup passou sem warnings. O schema do banco ficou alinhado com o modelo de código. Todos os 49 testes de integração do Atendimento passaram, incluindo os que sobem um banco in-memory com migrations aplicadas.

**Conceito transmissível:** o EF Core mantém um "snapshot" do modelo no arquivo `ModelSnapshot.cs` dentro do projeto de Infrastructure. Qualquer remoção ou adição de entidade/propriedade precisa ser capturada em uma nova migration — não basta deletar o código. Sempre rodar `dotnet build` antes de `dotnet ef migrations add`.

---

## EXP-004 — CPF inválido rejeitado por validação matemática real

### Situação

Ao testar manualmente a API de criação de cliente com o CPF `12345678901` (valor escolhido por ser fácil de lembrar), a API retornava HTTP 400 com `"Documento": ["CPF inválido"]`. O campo era tratado como CPF por ter 11 dígitos, mas a validação falhava mesmo assim.

### Tarefa

Entender por que o CPF era rejeitado e encontrar um valor válido para usar nos testes e na collection Postman.

### Ação

A investigação no código revelou que o domínio implementava validação real do CPF com o algoritmo dos dígitos verificadores (os dois últimos dígitos do CPF são calculados matematicamente a partir dos nove primeiros). O CPF `12345678901` não passa nessa validação porque os dígitos `01` não correspondem ao cálculo esperado para `123456789`.

```csharp
// Algoritmo resumido
int soma1 = digits[0]*10 + digits[1]*9 + ... + digits[8]*2;
int dig1 = soma1 % 11 < 2 ? 0 : 11 - soma1 % 11;
// dig1 deve ser igual a digits[9]
```

A solução foi usar o CPF `52998224725`, que é:
- Matematicamente válido (passa nos dois dígitos verificadores)
- Fictício (não pertence a nenhuma pessoa real)
- Gerado por ferramentas de geração de CPF de teste

### Resultado

A troca do CPF nos exemplos da collection Postman e nos scripts de teste eliminou o erro 400. A validação no domínio foi confirmada como correta — ela protege contra dados sem sentido chegando ao banco, o que é o comportamento esperado em uma fronteira de entrada do sistema.

**Conceito transmissível:** CPF não é um número qualquer de 11 dígitos — tem dígitos verificadores calculados por algoritmo. Em testes e demos, sempre usar CPFs gerados por ferramentas específicas (ou um conjunto fixo de CPFs fictícios conhecidos). Usar sequências como `12345678901` vai falhar em qualquer sistema com validação correta. O mesmo vale para CNPJ e outros documentos com dígito verificador.

---

## EXP-005 — Dois catálogos de peças gerando IDs incompatíveis entre microserviços

### Situação

O Atendimento tinha seu próprio cadastro de peças (`POST /atendimento/pecas`) e o Estoque também tinha o seu (`POST /estoque/pecas`). Para o fluxo de baixa automática funcionar, o `PecaId` gravado nos `ItensPeca` da OS precisava ser o mesmo ID que existia no Estoque. Na prática, isso exigia que o operador cadastrasse a peça nos dois serviços e que ambos gerassem o mesmo GUID — o que era impossível por padrão (cada `new Guid()` é único).

### Tarefa

Eliminar a necessidade de sincronização manual de IDs entre os dois serviços, garantindo que o `PecaId` na OS sempre fosse válido para o consumer do Estoque.

### Ação

A decisão arquitetural (ADR-004) foi: **Estoque é a única fonte da verdade para peças**. O catálogo do Atendimento foi completamente removido — entidade, repositório, use case, controller e todos os testes associados.

O Atendimento passou a consultar o Estoque via HTTP quando precisa de dados de uma peça:

```csharp
// IEstoquePort — porta de saída (Application)
Task<PecaEstoqueDto?> ObterPecaAsync(Guid pecaId, CancellationToken ct = default);

// EstoqueHttpAdapter — adaptador real (Infrastructure)
var response = await httpClient.GetAsync($"/estoque/pecas/{pecaId}", ct);
if (!response.IsSuccessStatusCode) return null;
return await response.Content.ReadFromJsonAsync<PecaEstoqueDto>(JsonOptions, ct);
```

O impacto no código foi significativo: remoção de 7 arquivos do Atendimento, nova migration para dropar a tabela `Pecas`, ajuste nos testes de integração para usar `FakeEstoqueAdapter` no lugar do banco local.

A proteção contra indisponibilidade do Estoque já existia: o `HttpClient` era configurado com Polly (retry + circuit breaker), então o Atendimento degrada graciosamente se o Estoque estiver fora.

### Resultado

O problema de IDs incompatíveis foi eliminado estruturalmente — não há mais como um operador criar uma OS com um `PecaId` que não existe no Estoque, porque o ID vem diretamente do Estoque. O fluxo end-to-end (OS finalizada → evento RabbitMQ → baixa no estoque) passou a funcionar de ponta a ponta com a mesma peça.

**Conceito transmissível:** dados duplicados em microsserviços diferentes são uma das principais fontes de inconsistência. A regra é: cada dado tem exatamente um dono (um serviço é a fonte da verdade). Os outros serviços podem cachear ou referenciar, mas nunca manter uma cópia gerenciável de forma independente. Quando você se pegar sincronizando IDs manualmente entre dois sistemas, é sinal de que a fronteira de responsabilidade está errada.

---

## EXP-006 — Endpoint inexistente na documentação (POST /auth/registrar)

### Situação

A collection Postman inicial incluía uma requisição `POST /auth/registrar` com body de nome, email, senha e perfil. Ao importar e executar, a requisição retornava HTTP 404. O endpoint simplesmente não existia na API.

### Tarefa

Entender como usuários eram criados no sistema e corrigir a documentação para refletir o contrato real.

### Ação

Lendo o `AuthController`, ele tinha apenas `POST /auth/login`. O `UsuariosController` (rota `/usuarios`) tinha `POST /usuarios` com os campos corretos para criação. A confusão veio de um padrão comum em outras APIs onde `/auth/register` é o endpoint de cadastro — mas neste sistema, a criação de usuário é uma operação administrativa (requer perfil Admin) separada da autenticação.

A correção foi remover o endpoint inexistente da collection e documentar o fluxo correto: usuário admin é criado automaticamente no seed do banco (`admin@oficina.com / Admin@123`), e novos usuários são criados via `POST /usuarios` com token de Admin.

### Resultado

A collection ficou alinhada com o código real. O passo de "criar usuário" na documentação passou a usar a rota correta, e a descrição esclareceu a distinção entre autenticação (`/auth`) e gestão de usuários (`/usuarios`).

**Conceito transmissível:** documentação que não foi gerada a partir do código real (ex.: Swagger auto-gerado) envelhece e fica errada. A única documentação confiável é a derivada do código — Swagger, contratos testados ou collection gerada programaticamente. Ao criar documentação manual, sempre cruzar cada rota com o controller real antes de publicar.

---

## EXP-007 — Liveness probe derrubando Pod do RabbitMQ antes de inicializar

### Situação

Ao aplicar o StatefulSet do RabbitMQ no Minikube, o Pod entrava em loop de restart (`RESTARTS: 1, 2...`). O status mostrava `Running` mas `0/1` — o container estava vivo mas nunca passava no readiness. O `kubectl describe` revelou: `Liveness probe failed: command timed out: "rabbitmq-diagnostics ping" timed out after 1s`.

### Tarefa

Entender por que a liveness probe estava matando o Pod antes do RabbitMQ terminar de inicializar e corrigir os parâmetros sem remover a probe.

### Ação

O problema tinha duas causas sobrepostas:

1. **`timeoutSeconds` padrão é 1s** — o comando `rabbitmq-diagnostics ping` no Minikube (ambiente com menos recursos que produção) demora mais de 1s para responder, causando timeout mesmo quando o broker estava saudável.

2. **`initialDelaySeconds: 60` insuficiente** — o RabbitMQ no Minikube levava mais de 60s para estar totalmente pronto, então a liveness começava a checar antes do broker responder.

A correção foi ajustar ambos os parâmetros:

```yaml
readinessProbe:
  exec:
    command: ["rabbitmq-diagnostics", "ping"]
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 10      # de 1s para 10s
  failureThreshold: 6

livenessProbe:
  exec:
    command: ["rabbitmq-diagnostics", "ping"]
  initialDelaySeconds: 120  # de 60s para 120s
  periodSeconds: 15
  timeoutSeconds: 10        # de 1s para 10s
  failureThreshold: 3
```

Após aplicar o manifesto atualizado, o Pod não recriou automaticamente com os novos valores — o StatefulSet não força recriação do Pod ao ser atualizado. Foi necessário deletar o Pod manualmente para o StatefulSet recriar com as novas configurações:

```powershell
kubectl delete pod rabbitmq-0 -n oficina-mecanica
```

### Resultado

Após a recriação com os novos timeouts, o `rabbitmq-0` subiu para `1/1 Running` sem restarts. A Management UI ficou acessível via port-forward confirmando o broker saudável.

**Conceito transmissível:** probes com `timeoutSeconds: 1` (padrão) são frágeis em ambientes locais com menos recursos (Minikube, CI) e em aplicações que levam tempo para inicializar. Sempre ajustar `timeoutSeconds` e `initialDelaySeconds` de acordo com o comportamento real da aplicação no ambiente alvo — não apenas no ambiente de produção. Além disso, StatefulSets não recriam Pods automaticamente ao serem atualizados: é necessário deletar o Pod manualmente para aplicar novos valores de probe.

---

## EXP-008 — NodePort inacessível pelo IP do nó no Minikube com driver Docker no Windows

### Situação

Após subir o Deployment do Atendimento com Service do tipo NodePort na porta 30080, o health check via `http://192.168.49.2:30080/health` retornava "Impossível conectar-se ao servidor remoto". Os Pods estavam `1/1 Running` e os logs mostravam o serviço escutando normalmente na porta 8080.

### Tarefa

Entender por que o NodePort não estava acessível pelo IP do nó e encontrar a forma correta de acessar serviços do Minikube no Windows.

### Ação

O Minikube com `--driver=docker` no Windows cria o cluster dentro de um container Docker. O IP `192.168.49.2` é o IP interno da rede Docker — não é roteável diretamente pelo Windows Host, diferente do que acontece no Linux onde o IP do nó é acessível diretamente.

A solução é usar o comando nativo do Minikube que cria um túnel local:

```powershell
minikube service atendimento-svc -n oficina-mecanica --url
# retorna: http://127.0.0.1:63934
# ! Because you are using a Docker driver on windows, the terminal needs to be open to run it.
```

O terminal precisa ficar aberto mantendo o túnel ativo. O teste é feito em um segundo terminal usando a URL retornada.

### Resultado

Com o túnel ativo, `Invoke-RestMethod http://127.0.0.1:63934/health` retornou `Healthy`. O mesmo padrão foi aplicado ao Estoque (porta diferente a cada execução).

**Conceito transmissível:** no Minikube com driver Docker no Windows, o IP do nó (`minikube ip`) não é acessível diretamente — é necessário usar `minikube service <nome> --url` para criar um túnel. Em Linux isso não é necessário. Em produção (EKS, GKE) o NodePort é substituído por LoadBalancer ou Ingress, que expõem IPs acessíveis externamente de forma nativa.

---

## EXP-009 — Token JWT não persiste entre sessões do PowerShell

### Situação

Ao executar o fluxo de testes end-to-end do Kubernetes em múltiplos comandos PowerShell separados, o token JWT obtido no login não estava disponível nos comandos seguintes. As requisições retornavam HTTP 401.

### Tarefa

Executar o fluxo completo de autenticação e chamadas à API em uma única sessão sem perder o token entre os passos.

### Ação

Variáveis no PowerShell vivem apenas na sessão (processo) atual. Cada bloco de comando executado em uma nova invocação do PowerShell começa sem as variáveis definidas anteriormente. A solução foi consolidar todo o fluxo — login, criação de entidades e transições de status — em um único bloco de script executado de uma vez:

```powershell
# Login e captura do token
$login = Invoke-RestMethod -Uri "$atendimento/auth/login" -Method POST ...
$token = $login.token
$h = @{ Authorization = "Bearer $token"; "Content-Type" = "application/json" }

# Todos os passos seguintes usando $h na mesma sessão
$peca   = Invoke-RestMethod -Uri "$estoque/estoque/pecas" -Headers $h ...
$cliente = Invoke-RestMethod -Uri "$atendimento/clientes" -Headers $h ...
# ...
```

### Resultado

O fluxo completo executou sem erros de autenticação: login → criar peça → criar cliente → criar veículo → abrir OS → transições de status → finalizar → verificar estoque reduzido de 10 para 8 com 0 falhas registradas.

**Conceito transmissível:** variáveis de ambiente e de sessão têm escopo limitado ao processo. Em scripts de automação e testes, sempre executar fluxos encadeados em um único script ou pipeline — nunca assumir que uma variável definida em um comando anterior estará disponível no próximo. O mesmo princípio vale para tokens JWT: eles são credenciais de sessão e devem ser passados explicitamente em cada requisição.

---

## Referência rápida

| # | Problema | Tecnologia | Conceito-chave |
|---|---|---|---|
| EXP-001 | Saldo negativo em concorrência | EF Core | Concorrência otimista / RowVersion |
| EXP-002 | Consumer RabbitMQ não executava | MassTransit | `messageType` = namespace + classe |
| EXP-003 | Startup bloqueado por modelo divergente | EF Core Migrations | Snapshot vs. modelo em memória |
| EXP-004 | CPF fictício rejeitado | Domain validation | Dígito verificador CPF |
| EXP-005 | IDs de peça incompatíveis entre serviços | Microserviços | Fonte única da verdade |
| EXP-006 | Endpoint documentado mas inexistente | ASP.NET Core | Documentação derivada do código |
| EXP-007 | Liveness probe derrubando Pod no Minikube | Kubernetes | `timeoutSeconds` e `initialDelaySeconds` |
| EXP-008 | NodePort inacessível no Windows com Minikube | Kubernetes / Minikube | `minikube service --url` como túnel |
| EXP-009 | Token JWT perdido entre sessões PowerShell | PowerShell / Testes | Escopo de variáveis de sessão |
