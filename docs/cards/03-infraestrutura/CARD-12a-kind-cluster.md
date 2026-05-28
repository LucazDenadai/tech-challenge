# CARD-12.1 — Cluster local com Kind

**Depende de:** Docker Desktop rodando  
**Bloqueia:** CARD-12.2

---

## Contexto

Kind roda um cluster Kubernetes inteiro dentro de containers Docker — 1 container por nó. É o padrão para desenvolvimento local: você controla a versão, pode destruir e recriar em segundos.

---

## Tarefas

**1. Instalar Kind**

```powershell
winget install Kubernetes.kind
# fechar e reabrir o terminal
kind version
```

**2. Criar o arquivo de configuração do cluster**

```yaml
# k8s/kind-config.yaml
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
  - role: worker
```

**3. Criar o cluster**

```powershell
kind create cluster --name oficina --config k8s/kind-config.yaml
```

---

## Validação

```powershell
kubectl config current-context   # deve retornar: kind-oficina
kubectl get nodes                # ambos os nós em STATUS=Ready
```

---

## Entregável

- [ ] `kind version` sem erro
- [ ] 2 nós em `Ready`
- [ ] Contexto ativo: `kind-oficina`
