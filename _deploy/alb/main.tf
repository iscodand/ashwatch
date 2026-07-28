terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
  }

  backend "s3" {
    bucket       = "ashwatch-tf-state-bucket"
    key          = "alb/terraform.tfstate"
    region       = "us-east-1"
    use_lockfile = true
  }
}

provider "aws" {
  region = "us-east-1"
}

data "terraform_remote_state" "network" {
  backend = "s3"
  config = {
    bucket = "ashwatch-tf-state-bucket"
    key    = "network/terraform.tfstate"
    region = "us-east-1"
  }
}

resource "aws_lb" "ashwatch_lb" {
  name               = "ashwatchlb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = data.terraform_remote_state.network.outputs.alb_sg_id
  subnets            = data.terraform_remote_state.network.outputs.public_subnet_ids
}

resource "aws_lb_target_group" "ashwatch_command_lb" {
  name        = "ashwatch-command-api-lb"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = data.terraform_remote_state.network.outputs.vpc_id
  target_type = "ip"

  health_check {
    path                = "/health"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
    matcher             = "200"
  }
}

resource "aws_lb_target_group" "ashwatch_query_lb" {
  name        = "ashwatch-query-api-lb"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = data.terraform_remote_state.network.outputs.vpc_id
  target_type = "ip"

  health_check {
    path                = "/health"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
    matcher             = "200"
  }
}

resource "aws_lb_listener" "ashwatch_lb_listener" {
  load_balancer_arn = aws_lb.ashwatch_lb.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = "Not Found"
      status_code  = "404"
    }
  }
}

resource "aws_lb_listener_rule" "ashwatch_command_listener_rule" {
  listener_arn = aws_lb_listener.ashwatch_lb_listener.arn
  priority     = 100

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.ashwatch_command_lb.arn
  }

  condition {
    path_pattern {
      values = ["/commands/*"]
    }
  }
}

resource "aws_lb_listener_rule" "ashwatch_query_listener_rule" {
  listener_arn = aws_lb_listener.ashwatch_lb_listener.arn
  priority     = 200

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.ashwatch_query_lb.arn
  }

  condition {
    path_pattern {
      values = ["/query/*"]
    }
  }
}
