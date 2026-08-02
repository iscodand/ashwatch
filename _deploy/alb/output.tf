output "command_target_group_arn" {
  value = aws_lb_target_group.ashwatch_command_lb.arn
}

output "query_target_group_arn" {
  value = aws_lb_target_group.ashwatch_query_lb.arn
}
