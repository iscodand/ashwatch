output "worker_queue_arn" {
  value = aws_sqs_queue.ashwatch-sqs.arn
}

output "logging_topic_arn" {
  value = aws_sns_topic.ashwatch-logging-sns.arn
}
