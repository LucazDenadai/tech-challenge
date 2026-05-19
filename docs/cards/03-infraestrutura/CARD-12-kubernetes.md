# CARD-12 — Kubernetes: manifestos YAML

**Tipo:** Infra  
**Status:** To Do  
**Depende de:** CARD-11  
**Bloqueia:** CARD-14 (CI/CD deploy)

---

## Contexto

Criar os manifestos Kubernetes para deploy de todos os componentes do sistema. O cluster pode ser local (Kind, Minikube, Docker Desktop K8s) ou cloud. Os manifestos devem ser aplicáveis com `kubectl apply -f k8s/`.

---

## Critérios de aceite

- [ ] `kubectl apply -f k8s/` sobe todos os recursos sem erro
- [ ] Atendimento e Estoque acessíveis via Service
- [ ] HPA configurado para ambos os serviços (escala por CPU)
- [ ] Secrets usados para JWT, credenciais de banco e SMTP (não ConfigMap)
- [ ] ConfigMaps para configurações não sensíveis
- [ ] Todos os Pods com `readinessProbe` e `livenessProbe`
- [ ] RabbitMQ com PersistentVolumeClaim

---

## Estrutura de arquivos

```
k8s/
├── namespace.yaml
├── postgres/
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── pvc.yaml
│   └── secret.yaml
├── rabbitmq/
│   ├── statefulset.yaml
│   ├── service.yaml
│   └── pvc.yaml
├── atendimento/
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── configmap.yaml
│   ├── secret.yaml
│   └── hpa.yaml
└── estoque/
    ├── deployment.yaml
    ├── service.yaml
    ├── configmap.yaml
    ├── secret.yaml
    └── hpa.yaml
```

---

## Especificações dos recursos

### Deployments

```yaml
# Atendimento e Estoque seguem o mesmo padrão
replicas: 2
resources:
  requests:
    cpu: "100m"
    memory: "128Mi"
  limits:
    cpu: "500m"
    memory: "256Mi"
```

### HPA — Horizontal Pod Autoscaler

```yaml
# k8s/atendimento/hpa.yaml
minReplicas: 2
maxReplicas: 10
metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Secrets (não commitar valores reais)

```yaml
# k8s/atendimento/secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: atendimento-secrets
type: Opaque
data:
  JWT_KEY: <base64>
  DB_PASSWORD: <base64>
  SMTP_PASSWORD: <base64>
```

> Valores reais injetados pelo CI/CD via `kubectl create secret` com variáveis do repositório (CARD-14).

### Probes

```yaml
readinessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
livenessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10
```

---

## Passos

1. Criar `namespace.yaml` (`oficina-mecanica`)
2. Criar manifestos do PostgreSQL com PVC
3. Criar manifestos do RabbitMQ com StatefulSet e PVC
4. Criar manifestos do Atendimento (Deployment, Service, ConfigMap, Secret, HPA)
5. Criar manifestos do Estoque (Deployment, Service, ConfigMap, Secret, HPA)
6. Adicionar endpoint `/health` nos dois serviços (ASP.NET Core Health Checks)
7. Validar com `kubectl apply -f k8s/` em cluster local
8. Validar HPA com `kubectl get hpa`
