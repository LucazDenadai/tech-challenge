output "container_name" {
  description = "Nome do container PostgreSQL (usado como hostname dentro da rede Kind)"
  value       = docker_container.postgres.name
}

output "connection_string" {
  description = "Connection string para uso nas aplicações (host = nome do container na rede Kind)"
  value       = "Host=${docker_container.postgres.name};Port=5432;Database=${var.db_name};Username=${var.db_user};Password=${var.db_password}"
  sensitive   = true
}

output "host_connection_string" {
  description = "Connection string para acesso externo pelo host (DBeaver, psql local)"
  value       = "Host=localhost;Port=${var.db_port};Database=${var.db_name};Username=${var.db_user};Password=${var.db_password}"
  sensitive   = true
}
