# Roteiro — Vídeo de Demonstração (até 15 min)

**Projeto:** Tech Challenge Fase 2 — Oficina Mecânica  
**Tempo alvo:** 13–14:30 minutos  
**Ferramenta de gravação:** OBS / Loom / Teams (qualquer uma)

---

## Preparação antes de gravar

Abrir antecipadamente e deixar visível:

- **Terminal A** — para comandos kubectl e o script de carga
- **Terminal B** — para `kubectl get hpa -n oficina-mecanica -w` (watch ao vivo)
- **Navegador — aba 1** — Grafana: `http://localhost:30300`
- **Navegador — aba 2** — Swagger Atendimento: `http://localhost:8080/swagger`
- **Navegador — aba 3** — RabbitMQ Management: `http://localhost:15672`
- **VS Code / IDE** — com a estrutura de pastas aberta

Confirmar que tudo está rodando:

```powershell
kubectl get pods -n oficina-mecanica
kubectl get pods -n observabilidade
```

Todos os pods devem estar `Running` antes de iniciar a gravação.

---

## Bloco 1 — Apresentação (0:00 – 1:30)

**O que mostrar:** tela do README no GitHub ou IDE.

**O que falar:**

> "Estamos apresentando o Tech Challenge Fase 2 — evolução do sistema de gestão de uma oficina mecânica. O objetivo desta fase foi garantir qualidade, resiliência e escalabilidade.
>
> O sistema é composto por dois microsserviços independentes: **Atendimento**, que gerencia clientes, veículos e ordens de serviço; e **Estoque**, que controla peças e movimentações.
>
> A comunicação entre eles é **assíncrona via RabbitMQ**: quando uma OS é finalizada, o Atendimento publica um evento que o Estoque consome para dar baixa automática nas peças utilizadas.
>
> Toda a infraestrutura está em **Kubernetes**, provisionada com **Terraform**, e o deploy é automatizado via **GitHub Actions**."

Mostrar rapidamente: diagrama de arquitetura no README → diagrama hexagonal → diagrama K8s.

---

## Bloco 2 — Pipeline CI/CD via Pull Request (1:30 – 4:00)

**O que mostrar:** GitHub — abrir um PR real, depois a aba Actions do repositório.

**O que falar:**

> "Vou abrir um Pull Request de verdade para mostrar o pipeline disparando a partir de uma mudança real."

Abrir o PR (branch já preparada com antecedência, ex.: pequeno ajuste ou o próprio card em andamento):

```powershell
gh pr create --title "CARD-XX: ..." --body "..."
```

> "Assim que o PR abre, o pipeline dispara automaticamente. São três jobs em sequência obrigatória: build-and-test, docker e deploy."

Mostrar a Action iniciando na aba **Checks** do PR ou em **Actions**.

> **[NOTA DE EDIÇÃO — cortar aqui]** Pausar a gravação enquanto o pipeline roda (build + testes + Testcontainers podem levar alguns minutos). Retomar a gravação já com os três jobs concluídos e verdes, para não gerar tempo morto no vídeo final.

Retomando com os jobs verdes:

1. **build-and-test** — `.NET 10`, testes unitários e de integração com Testcontainers, análise SonarCloud
2. **docker** — build e push das imagens para o GitHub Container Registry (GHCR)
3. **deploy** — self-hosted runner com acesso ao cluster local, aplica os manifestos K8s

> "Pipeline verde. Percebam que as actions estão fixadas por hash de commit — não por tag — o que garante rastreabilidade e evita supply chain attacks."

Mostrar o `dotnet.yml` rapidamente, apontar o `needs:` encadeado e o `self-hosted` no job de deploy.

Mergear o PR:

```powershell
gh pr merge --squash
```

> "Com o merge, o deploy no cluster acontece automaticamente. Vamos ver o resultado completo mais adiante."

---

## Bloco 3 — Infraestrutura (Terraform + Kubernetes) (4:00 – 6:00)

**O que mostrar:** Terminal A + estrutura de pastas.

**O que falar:**

> "A infraestrutura é provisionada com Terraform. Dois módulos: um cria o cluster Kind com os namespaces, outro sobe o PostgreSQL na rede Docker do Kind."

Mostrar a estrutura `infra/`:

```powershell
ls infra/
ls infra/modules/
```

> "Com um único comando o ambiente inteiro é provisionado do zero:"

```powershell
# (não executar ao vivo — mostrar o resultado já pronto)
# terraform -chdir=infra apply -auto-approve
```

Agora mostrar o Kubernetes:

```powershell
kubectl get all -n oficina-mecanica
kubectl get hpa -n oficina-mecanica
```

> "Dois deployments, cada um com 2 réplicas mínimas e até 10 máximas. O HPA está configurado para escalar quando CPU ultrapassar 70% ou memória 80%. Vamos ver isso em ação em breve."

---

## Bloco 4 — Fluxo de Negócio completo (6:00 – 9:00)

**O que mostrar:** Terminal A executando o script + Grafana aberto.

**O que falar:**

> "Vou demonstrar o ciclo completo de uma ordem de serviço usando o script de demonstração."

Executar:

```powershell
.\scripts\demo-carga.ps1 -Modo Fluxo
```

Narrar cada passo enquanto o script roda:

- **Peça no Estoque criada** → "O Estoque é a fonte da verdade para peças — decisão registrada no ADR-004. O Atendimento consulta disponibilidade via HTTP antes de abrir a OS."
- **Cliente + Veículo + Serviço criados** → "Todos os dados do domínio são validados — CPF com dígito verificador, placa no formato Mercosul."
- **OS aberta** → "Status inicial: Recebida. A máquina de estados protege as transições — não é possível pular etapas."
- **Status avançando** → mostrar no terminal a sequência: Recebida → EmDiagnostico → AguardandoAprovacao
- **Aprovação de orçamento** → "Este é o endpoint de notificação externa previsto no enunciado."
- **Finalizada** → "Aqui o evento `OsFinalizadaEvent` é publicado no RabbitMQ."

Mudar para a aba do **RabbitMQ Management**:

> "Podemos ver a fila `estoque.baixa` e o evento sendo consumido em tempo real."

Voltar ao terminal — o script aguarda 3s e mostra o estoque atualizado:

> "O consumer do Estoque processou o evento via MassTransit e deu baixa automática na peça. Isso é comunicação assíncrona funcionando de ponta a ponta."

---

## Bloco 5 — Escalabilidade (HPA) (9:00 – 12:00)

**O que mostrar:** aba Actions (retomando o PR do Bloco 2) → Terminal B com watch do HPA + Grafana Dashboard "APIs" + Terminal A com o script de carga.

**O que falar:**

> "Voltando ao PR que mesclamos há pouco: o job de deploy já concluiu e aplicou os manifestos no cluster."

Mostrar rapidamente o job **deploy** verde na Action e o resultado:

```powershell
kubectl get pods -n oficina-mecanica
```

> "Deploy completo e automatizado, sem intervenção manual. Agora vou simular um pico de demanda para demonstrar a escalabilidade automática."

Antes de rodar, mostrar o Grafana — dashboards "Oficina Mecânica — APIs" e "Oficina Mecânica — Kubernetes & HPA". Mostrar que o RPS está baixo e as réplicas estão em 2.

No **Terminal B** (já rodando):
```powershell
kubectl get hpa -n oficina-mecanica -w
```

No **Terminal A**, iniciar a carga:
```powershell
.\scripts\demo-carga.ps1 -Modo Carga -QtdOS 80
```

Narrar enquanto a carga sobe:

> "Oitenta ordens de serviço sendo criadas em paralelo. Observem o painel do Grafana — o RPS está subindo, a latência P95 também."

Quando o HPA reagir (Terminal B mostrando réplicas aumentando):

> "O HPA detectou o aumento de CPU e está escalando — de 2 para N réplicas automaticamente, sem intervenção manual. Isso é escalabilidade dinâmica."

Mostrar no Grafana o painel "Histórico de escalonamento HPA" com a linha de réplicas subindo.

Quando a carga terminar:

> "Com a carga encerrada, o HPA vai reduzir as réplicas gradualmente de volta ao mínimo de 2. Isso evita custo desnecessário de infraestrutura em períodos de baixa demanda."

Aguardar alguns segundos e mostrar as réplicas voltando a cair no Terminal B.

---

## Bloco 6 — Observabilidade (12:00 – 13:30)

**O que mostrar:** Grafana — alternar entre os dois dashboards.

**O que falar:**

> "Toda a stack de observabilidade está provisionada no namespace `observabilidade`: Prometheus coletando métricas, Loki agregando logs, Promtail fazendo o scrape dos pods, e Jaeger para rastreamento distribuído."

Mostrar rapidamente cada painel:

- **RPS e erros** → "Requisições por segundo por serviço. Nenhum erro 5xx durante a carga."
- **Latência P50/P95/P99** → "Dentro do esperado mesmo sob carga."
- **Painel de logs** → "Logs de todos os pods em tempo real, filtráveis por serviço."
- **Dashboard K8s** → "CPU e memória por pod. Mostra claramente o impacto da carga e o retorno ao estado normal."

---

## Bloco 7 — Encerramento (13:30 – 14:30)

**O que mostrar:** README no GitHub.

**O que falar:**

> "Para resumir o que foi entregue nesta fase:
>
> - Dois microsserviços com **Arquitetura Hexagonal** — Domain sem dependências externas, Application sem referência a Infrastructure
> - Comunicação assíncrona via RabbitMQ com retry automático e dead letter queue
> - **7 ADRs** documentando todas as decisões arquiteturais com alternativas rejeitadas
> - Testes unitários e de integração com Testcontainers — banco real, sem mocks
> - Kubernetes com HPA, health checks e stack de observabilidade completa
> - Infraestrutura como Código com Terraform
> - Pipeline CI/CD com GitHub Actions — build, testes, Docker e deploy em sequência
>
> O código, os manifestos e a documentação estão no repositório compartilhado com o usuário `soat-architecture`. Obrigado."

---

## Checklist final antes de publicar

- [ ] Vídeo tem até 15 minutos
- [ ] Todos os blocos estão cobertos
- [ ] Branch/PR preparado com antecedência para abrir ao vivo no Bloco 2
- [ ] PR aberto, pipeline disparado e mesclado na gravação (com corte de edição na espera do build)
- [ ] HPA escalando é visível na gravação
- [ ] Fluxo completo de OS (abrir → finalizar → baixa de estoque) demonstrado
- [ ] Pipeline CI/CD mostrado com jobs verdes (do PR real, não de execução antiga)
- [ ] Link do vídeo adicionado no README
- [ ] PDF entregue no portal com: link do repositório + desenho da arquitetura + link do vídeo
