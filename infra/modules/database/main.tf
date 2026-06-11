terraform {
  required_providers {
    docker = {
      source  = "kreuzwerker/docker"
      version = "~> 3.0"
    }
    null = {
      source  = "hashicorp/null"
      version = "~> 3.2"
    }
  }
}

# Busca a rede Docker criada pelo Kind
# data "docker_network" lê um recurso existente sem criar — só consulta
data "docker_network" "kind" {
  name = var.kind_network
}

resource "docker_image" "postgres" {
  name         = "postgres:16"
  keep_locally = true
}

resource "docker_container" "postgres" {
  name  = "oficina-postgres"
  image = docker_image.postgres.image_id

  # Variáveis de ambiente que o container oficial do Postgres usa
  # para criar o banco e o usuário na primeira inicialização
  env = [
    "POSTGRES_DB=${var.db_name}",
    "POSTGRES_USER=${var.db_user}",
    "POSTGRES_PASSWORD=${var.db_password}",
  ]

  # Porta 5432 do container acessível na porta var.db_port do host
  # Útil para conectar via DBeaver ou psql direto da sua máquina
  ports {
    internal = 5432
    external = var.db_port
  }

  # Conecta o container à rede do Kind para que os pods consigam alcançá-lo
  networks_advanced {
    name = data.docker_network.kind.name
  }

  # Garante que o container reinicia automaticamente se o Docker reiniciar
  restart = "unless-stopped"

  healthcheck {
    test         = ["CMD-SHELL", "pg_isready -U ${var.db_user} -d ${var.db_name}"]
    interval     = "5s"
    timeout      = "5s"
    retries      = 10
    start_period = "10s"
  }
}

# null_resource não cria infraestrutura real — serve para executar
# scripts locais (local-exec) ou remotos como parte do apply.
# Aqui usamos para criar os schemas após o container estar saudável.
resource "null_resource" "create_schemas" {
  depends_on = [docker_container.postgres]

  # triggers define quando esse null_resource deve re-executar.
  # Se o container for recriado (novo ID), o script roda de novo.
  triggers = {
    container_id = docker_container.postgres.id
  }

  provisioner "local-exec" {
    # Aguarda o Postgres aceitar conexões, depois cria os dois schemas.
    # O loop evita falha se o container ainda estiver inicializando.
    # Usa PowerShell pois o ambiente é Windows.
    interpreter = ["PowerShell", "-Command"]
    command     = <<-EOT
      Write-Host "Aguardando PostgreSQL ficar pronto..."
      $ready = $false
      for ($i = 1; $i -le 20; $i++) {
        docker exec oficina-postgres pg_isready -U ${var.db_user} -d ${var.db_name}
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
        Write-Host "Tentativa $i/20 - aguardando 3s..."
        Start-Sleep -Seconds 3
      }
      if (-not $ready) { throw "PostgreSQL nao ficou pronto a tempo" }
      Write-Host "Criando schemas..."
      docker exec oficina-postgres psql -U ${var.db_user} -d ${var.db_name} -c "CREATE SCHEMA IF NOT EXISTS atendimento;"
      docker exec oficina-postgres psql -U ${var.db_user} -d ${var.db_name} -c "CREATE SCHEMA IF NOT EXISTS estoque;"
      Write-Host "Schemas criados com sucesso."
    EOT
  }
}
