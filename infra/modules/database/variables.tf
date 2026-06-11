variable "db_name" {
  description = "Nome do banco de dados principal"
  type        = string
  default     = "oficinamecanica"
}

variable "db_user" {
  description = "Usuário do PostgreSQL"
  type        = string
  default     = "oficina"
}

variable "db_password" {
  description = "Senha do PostgreSQL (não use o default em produção)"
  type        = string
  sensitive   = true
}

variable "db_port" {
  description = "Porta exposta no host para acesso externo ao PostgreSQL"
  type        = number
  default     = 5433
}

variable "kind_network" {
  description = "Nome da rede Docker criada pelo Kind (sempre 'kind')"
  type        = string
  default     = "kind"
}
