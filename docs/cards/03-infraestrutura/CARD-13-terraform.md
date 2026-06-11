# CARD-13 — Terraform: infraestrutura como código

**Tipo:** Infra  
**Status:** To Do  
**Depende de:** CARD-12  
**Bloqueia:** CARD-14 (CI/CD)

---

## Contexto

Criar scripts Terraform para provisionar o cluster Kubernetes e o banco de dados. O objetivo é que qualquer pessoa do time consiga recriar o ambiente do zero com `terraform apply`. O alvo pode ser local (Kind via Terraform) ou cloud (EKS, GKE ou AKS).

---

## Critérios de aceite

- [ ] `terraform init` e `terraform apply` executam sem erro
- [ ] Cluster Kubernetes provisionado
- [ ] Banco de dados PostgreSQL provisionado com schemas `atendimento` e `estoque`
- [ ] Outputs documentados (endpoint do cluster, connection string do banco)
- [ ] `README` em `/infra` explicando os recursos criados e como aplicar
- [ ] `terraform destroy` desfaz tudo sem erro

---

## Abordagem recomendada: Kind local

Kind (Kubernetes in Docker) é a opção mais simples para o desafio — roda no Docker sem conta em cloud, demonstrável no vídeo, sem custo.

Se o time preferir cloud, usar **GKE Autopilot** (free tier) ou **EKS** (conta AWS existente).

---

## Estrutura de arquivos

```
infra/
├── main.tf
├── variables.tf
├── outputs.tf
├── versions.tf
└── modules/
    ├── cluster/
    │   ├── main.tf        # Kind cluster ou cloud K8s
    │   ├── variables.tf
    │   └── outputs.tf
    └── database/
        ├── main.tf        # PostgreSQL (container local ou RDS/Cloud SQL)
        ├── variables.tf
        └── outputs.tf
```

---

## Recursos provisionados

### Módulo `cluster`
- Cluster Kubernetes (Kind ou cloud)
- Namespace `oficina-mecanica`
- Service Account com permissões mínimas

### Módulo `database`
- Instância PostgreSQL
- Banco de dados `oficinamecanica`
- Schemas `atendimento` e `estoque`
- Usuário com senha (output sensível)

---

## Exemplo de uso (Kind local)

```hcl
# infra/main.tf
module "cluster" {
  source       = "./modules/cluster"
  cluster_name = "oficina-mecanica"
  k8s_version  = "v1.31.0"
}

module "database" {
  source      = "./modules/database"
  db_name     = "oficinamecanica"
  db_user     = var.db_user
  db_password = var.db_password
}
```

```bash
# Aplicar
cd infra
terraform init
terraform plan
terraform apply

# Destruir
terraform destroy
```

---

## README obrigatório em /infra

Deve conter:
1. Quais recursos são criados
2. Pré-requisitos (Docker, Kind, Terraform)
3. Como aplicar (`terraform init` → `terraform apply`)
4. Como destruir
5. Outputs e como usá-los

---

## Passos

1. Criar estrutura de pastas em `/infra`
2. Implementar módulo `cluster` (Kind ou cloud)
3. Implementar módulo `database`
4. Criar `variables.tf` com todas as variáveis documentadas
5. Criar `outputs.tf` com endpoint do cluster e connection string
6. Validar `terraform apply` do zero
7. Escrever `README.md` em `/infra`
