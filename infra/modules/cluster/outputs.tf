output "endpoint" {
  description = "Endereço do API server do cluster Kind"
  value       = kind_cluster.this.endpoint
}

output "kubeconfig" {
  description = "Conteúdo do kubeconfig para conectar ao cluster"
  value       = kind_cluster.this.kubeconfig
  sensitive   = true
}

output "cluster_name" {
  description = "Nome do cluster criado"
  value       = kind_cluster.this.name
}
