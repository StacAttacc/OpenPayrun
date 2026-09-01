terraform {
  required_version = ">= 1.6.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
  tenant_id       = var.tenant_id
}

locals {
  app_name = "scsi-tax-calculator"

  app_url = "https://${local.app_name}.${azurerm_container_app_environment.main.default_domain}"

  sql_connection_string = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Database=${azurerm_mssql_database.main.name};User Id=${var.sql_admin_username};Password=${var.sql_admin_password};Encrypt=True;TrustServerCertificate=False;"
}

resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "law-scsi-tax-calculator"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = var.tags
}

resource "azurerm_container_app_environment" "main" {
  name                       = "cae-scsi-tax-calculator"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  tags                       = var.tags

  # Azure attaches a default Consumption workload_profile that isn't in
  # this config; without this it shows as perpetual drift on every plan.
  lifecycle {
    ignore_changes = [workload_profile]
  }
}

resource "azurerm_mssql_server" "main" {
  name                         = "sql-scsi-tax-calculator"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
  minimum_tls_version          = "1.2"
  tags                         = var.tags
}

resource "azurerm_mssql_database" "main" {
  name        = "ScsiTaxCalculator"
  server_id   = azurerm_mssql_server.main.id
  sku_name    = "Basic"
  max_size_gb = 2
  tags        = var.tags
}

resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_container_app" "app" {
  name                         = local.app_name
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"
  tags                         = var.tags

  lifecycle {
    ignore_changes = [workload_profile_name]
  }

  secret {
    name  = "sql-connection-string"
    value = local.sql_connection_string
  }
  secret {
    name  = "admin-password"
    value = var.admin_password
  }
  secret {
    name  = "jwt-secret"
    value = var.jwt_secret
  }

  # nginx (frontend) is the only externally reachable container; it proxies
  # /api to the backend over localhost since both containers share the same
  # network namespace within this app.
  ingress {
    external_enabled = true
    target_port      = 80
    transport        = "auto"
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = 1
    max_replicas = 2

    # Container Apps bakes secretRef values into the revision at creation
    # time; changing a secret alone doesn't create a new revision. Hashing
    # the secrets into the suffix forces one whenever they rotate.
    revision_suffix = substr(md5("${var.admin_password}${var.jwt_secret}${var.sql_admin_password}"), 0, 10)

    container {
      name   = "backend"
      image  = "ghcr.io/stacattacc/scsi-tax-calculator:${var.backend_image_tag}"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name        = "ConnectionStrings__Default"
        secret_name = "sql-connection-string"
      }
      env {
        name  = "AllowedOrigins__0"
        value = local.app_url
      }
      env {
        name  = "Admin__Username"
        value = var.admin_username
      }
      env {
        name        = "Admin__Password"
        secret_name = "admin-password"
      }
      env {
        name        = "Jwt__Secret"
        secret_name = "jwt-secret"
      }
    }

    container {
      name   = "frontend"
      image  = "ghcr.io/stacattacc/scsi-tax-calculator-frontend:${var.frontend_image_tag}"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "API_URL"
        value = "http://localhost:8080"
      }
    }
  }
}
