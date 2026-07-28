output "db_endpoint" {
  value = aws_db_instance.ashwatch_db.address
}

output "db_port" {
  value = aws_db_instance.ashwatch_db.port
}

output "db_name" {
  value = aws_db_instance.ashwatch_db.db_name
}

output "db_secret_arn" {
  value = aws_secretsmanager_secret.ashwatch_db_secret.arn
}
