# Infraestrutura — Terraform

Provisiona o ambiente local de desenvolvimento usando Kind (Kubernetes in Docker) e PostgreSQL via Docker.

## O que é criado

| Recurso | Tipo | Descrição |
|---|---|---|
| Cluster Kind | Kubernetes | Cluster K8s local rodando dentro do Docker |
| Namespace `oficina-mecanica` | K8s Namespace | Onde os serviços de aplicação rodam |
| Namespace `observabilidade` | K8s Namespace | Reservado para Jaeger, Prometheus, Grafana |
| Container `oficina-postgres` | Docker Container | PostgreSQL 16 com schemas `atendimento` e `estoque` |

## Pré-requisitos

- [Docker](https://docs.docker.com/get-docker/) rodando
- [Terraform >= 1.6](https://developer.hashicorp.com/terraform/install)
- [Kind](https://kind.sigs.k8s.io/docs/user/quick-start/#installation) instalado

Verifique:
```bash
docker --version
terraform --version
kind --version
```

## Como usar

### 1. Configurar variáveis

```bash
cp terraform.tfvars.example terraform.tfvars
# Edite terraform.tfvars e defina db_password
```

### 2. Inicializar (baixa os providers — só na primeira vez)

```bash
terraform init
```

### 3. Ver o que será criado (não altera nada)

```bash
terraform plan
```

### 4. Provisionar tudo

```bash
terraform apply
```

Após o apply, o Terraform exibe os outputs. Para ver os valores sensíveis:

```bash
terraform output postgres_connection_string
terraform output postgres_host_connection_string
```

### 5. Aplicar os manifestos Kubernetes

Com o cluster criado, aplique os manifestos da aplicação:

```bash
kubectl apply -f ../k8s/ -n oficina-mecanica
```

### 6. Destruir tudo

```bash
terraform destroy
```

Isso remove o cluster Kind, o container PostgreSQL e todos os dados.

## Outputs

| Output | Descrição |
|---|---|
| `cluster_name` | Nome do cluster criado |
| `cluster_endpoint` | Endereço do API server |
| `postgres_connection_string` | Connection string para pods dentro do cluster |
| `postgres_host_connection_string` | Connection string para acesso pelo host |

## O que NÃO commitar

| Arquivo/Pasta | Motivo |
|---|---|
| `terraform.tfvars` | Contém senhas reais |
| `terraform.tfstate` | Estado da infra, pode conter dados sensíveis |
| `terraform.tfstate.backup` | Backup automático do state |
| `.terraform/` | Cache de providers (como node_modules) |

Todos já estão no `.gitignore` da raiz do projeto.
