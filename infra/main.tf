module "cluster" {
  source = "./modules/cluster"

  cluster_name       = var.cluster_name
  kubernetes_version = var.kubernetes_version
}

module "database" {
  source = "./modules/database"

  # O Kind sempre cria uma rede Docker chamada "kind"
  # O container Postgres entra nessa rede para que os pods consigam acessá-lo
  kind_network = "kind"

  db_name     = var.db_name
  db_user     = var.db_user
  db_password = var.db_password
  db_port     = var.db_port

  depends_on = [module.cluster]
}
