output "cluster_name" {
  description = "Nome do cluster Kind criado"
  value       = module.cluster.cluster_name
}

output "cluster_endpoint" {
  description = "Endereço do API server do cluster Kubernetes"
  value       = module.cluster.endpoint
}

output "postgres_connection_string" {
  description = "Connection string para as aplicações dentro do cluster"
  value       = module.database.connection_string
  sensitive   = true
}

output "postgres_host_connection_string" {
  description = "Connection string para acesso externo pelo host (DBeaver, psql)"
  value       = module.database.host_connection_string
  sensitive   = true
}
