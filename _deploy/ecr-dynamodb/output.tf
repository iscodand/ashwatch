output "worker_repository_url" {
  value = aws_ecr_repository.ashwatch-worker.repository_url
}

output "command_api_repository_url" {
  value = aws_ecr_repository.ashwatch-command-api.repository_url
}

output "query_api_repository_url" {
  value = aws_ecr_repository.ashwatch-query-api.repository_url
}

output "dynamodb_table_name" {
  value = aws_dynamodb_table.ashwatch.name
}
