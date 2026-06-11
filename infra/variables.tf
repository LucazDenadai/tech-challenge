variable "cluster_name" {
  description = "Nome do cluster Kind"
  type        = string
  default     = "oficina-mecanica"
}

variable "kubernetes_version" {
  description = "Versão da imagem do nó Kind"
  type        = string
  default     = "v1.31.0"
}

variable "db_name" {
  description = "Nome do banco de dados PostgreSQL"
  type        = string
  default     = "oficinamecanica"
}

variable "db_user" {
  description = "Usuário do PostgreSQL"
  type        = string
  default     = "oficina"
}

variable "db_password" {
  description = "Senha do PostgreSQL — obrigatório, não tem default"
  type        = string
  sensitive   = true
}

variable "db_port" {
  description = "Porta do PostgreSQL exposta no host"
  type        = number
  default     = 5433
}
