variable "cluster_name" {
  description = "Nome do cluster Kind"
  type        = string
  default     = "oficina-mecanica"
}

variable "kubernetes_version" {
  description = "Versão da imagem do nó Kind (deve existir em hub.docker.com/r/kindest/node)"
  type        = string
  default     = "v1.31.0"
}
