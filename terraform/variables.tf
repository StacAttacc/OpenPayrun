variable "subscription_id" {
  type = string
}

variable "tenant_id" {
  type = string
}

variable "resource_group_name" {
  type    = string
  default = "rg-scsi-tax-calculator"
}

variable "location" {
  type    = string
  default = "Canada Central"
}

variable "backend_image_tag" {
  type    = string
  default = "latest"
}

variable "frontend_image_tag" {
  type    = string
  default = "latest"
}

variable "sql_admin_username" {
  type    = string
  default = "scsiadmin"
}

variable "sql_admin_password" {
  type      = string
  sensitive = true
}

variable "admin_username" {
  type        = string
  description = "Username for the app's own admin login (tax-rate management)"
}

variable "admin_password" {
  type      = string
  sensitive = true
}

variable "jwt_secret" {
  type      = string
  sensitive = true
}

variable "tags" {
  type = map(string)
  default = {
    environment = "prod"
    managed-by  = "terraform"
    owner       = "scsi"
  }
}
