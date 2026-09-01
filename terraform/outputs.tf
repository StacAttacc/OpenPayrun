output "app_url" {
  value = "https://${azurerm_container_app.app.ingress[0].fqdn}"
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.main.fully_qualified_domain_name
}
