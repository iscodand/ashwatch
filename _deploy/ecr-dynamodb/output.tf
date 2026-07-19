output "worker_repository_url" {
  value = aws_ecr_repository.ashwatch-worker.repository_url
}

output "command_api_repository_url" {
  value = aws_ecr_repository.ashwatch-command-api.repository_url
}

output "query_api_repository_url" {
  value = aws_ecr_repository.ashwatch-query-api.repository_url
}

output "worker_repository_arn" {
  value = aws_ecr_repository.ashwatch-worker.arn
}

output "command_api_repository_arn" {
  value = aws_ecr_repository.ashwatch-command-api.arn
}

output "query_api_repository_arn" {
  value = aws_ecr_repository.ashwatch-query-api.arn
}

output "dynamodb_table_name" {
  value = aws_dynamodb_table.ashwatch.name
}

output "dynamodb_table_arn" {
  value = aws_dynamodb_table.ashwatch.arn
}
