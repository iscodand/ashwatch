output "vpc_id" {
  value = aws_vpc.main.id
}

output "private_subnet_ids" {
  value = [aws_subnet.private_a.id, aws_subnet.private_b.id]
}

output "public_subnet_ids" {
  value = [aws_subnet.public_a.id, aws_subnet.public_b.id]
}

output "alb_sg_id" {
  value = [aws_security_group.alb_sg.id]
}

output "fargate_sg_id" {
  value = aws_security_group.fargate_sg.id
}

output "aurora_sg_id" {
  value = aws_security_group.aurora_sg.id
}
