terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
  }

  backend "s3" {
    bucket       = "ashwatch-tf-state-bucket"
    key          = "fargate/terraform.tfstate"
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

data "terraform_remote_state" "ecr_dynamodb" {
  backend = "s3"
  config = {
    bucket = "ashwatch-tf-state-bucket"
    key    = "ecr-dynamodb/terraform.tfstate"
    region = "us-east-1"
  }
}

data "terraform_remote_state" "sns_sqs" {
  backend = "s3"
  config = {
    bucket = "ashwatch-tf-state-bucket"
    key    = "sns-sqs/terraform.tfstate"
    region = "us-east-1"
  }
}

data "terraform_remote_state" "db" {
  backend = "s3"
  config = {
    bucket = "ashwatch-tf-state-bucket"
    key    = "db/terraform.tfstate"
    region = "us-east-1"
  }
}

data "terraform_remote_state" "alb" {
  backend = "s3"
  config = {
    bucket = "ashwatch-tf-state-bucket"
    key    = "alb/terraform.tfstate"
    region = "us-east-1"
  }
}

resource "aws_ecs_cluster" "ashwatch-cluster" {
  name = "ashwatch_cluster"

  tags = {
    Name = "ashwatch-ecs-cluster"
  }
}

resource "aws_iam_role" "ecs_execution" {
  name               = "execution-role"
  assume_role_policy = data.aws_iam_policy_document.ecs_assume_role.json
}

resource "aws_iam_role" "command_api_task" {
  name               = "command_api_task"
  assume_role_policy = data.aws_iam_policy_document.ecs_assume_role.json
}

resource "aws_iam_role" "query_api_task" {
  name               = "query_api_task"
  assume_role_policy = data.aws_iam_policy_document.ecs_assume_role.json
}

data "aws_iam_policy_document" "ecs_assume_role" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

resource "aws_ecs_task_definition" "ashwatch_command_api" {
  family                   = "ashwatch-command-api"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = aws_iam_role.ecs_execution.arn
  task_role_arn            = aws_iam_role.command_api_task.arn

  container_definitions = jsonencode([
    {
      name      = "command-api"
      image     = "${data.terraform_remote_state.ecr_dynamodb.outputs.command_api_repository_url}:latest"
      essential = true

      portMappings = [
        {
          containerPort = 8080
          hostPort      = 8080
          protocol      = "tcp"
        }
      ]

      environment = [
        {
          name  = "SNS_TOPIC_ARN"
          value = data.terraform_remote_state.sns_sqs.outputs.logging_topic_arn
        },
        {
          name  = "DB_HOST"
          value = data.terraform_remote_state.db.outputs.db_endpoint
        },
        {
          name  = "DB_PORT",
          value = tostring(data.terraform_remote_state.db.outputs.db_port)
        },
        {
          name  = "DB_NAME",
          value = data.terraform_remote_state.db.outputs.db_name
        },
        {
          name  = "DB_PORT",
          value = tostring(data.terraform_remote_state.db.outputs.db_port)
        }
      ]

      secrets = [
        {
          name      = "DB_PASSWORD"
          valueFrom = "${data.terraform_remote_state.db.outputs.db_secret_arn}:password::"
        },
        {
          name      = "DB_USERNAME"
          valueFrom = "${data.terraform_remote_state.db.outputs.db_secret_arn}:username::"
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = "/ecs/ashwatch-command-api"
          "awslogs-region"        = "us-east-1"
          "awslogs-stream-prefix" = "ecs"
        }
      }
    }
  ])

  lifecycle {
    ignore_changes = [container_definitions]
  }
}

resource "aws_ecs_task_definition" "ashwatch_query_api" {
  family                   = "ashwatch-query-api"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = aws_iam_role.ecs_execution.arn
  task_role_arn            = aws_iam_role.query_api_task.arn

  container_definitions = jsonencode([
    {
      name      = "query-api"
      image     = "${data.terraform_remote_state.ecr_dynamodb.outputs.query_api_repository_url}:latest"
      essential = true

      portMappings = [
        {
          containerPort = 8080
          hostPort      = 8080
          protocol      = "tcp"
        }
      ]

      environment = [
        {
          name  = "DYNAMODB_TABLE"
          value = data.terraform_remote_state.ecr_dynamodb.outputs.dynamodb_table_name
        },
        {
          name  = "AWS_REGION"
          value = "us-east-1"
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = "/ecs/ashwatch-query-api"
          "awslogs-region"        = "us-east-1"
          "awslogs-stream-prefix" = "ecs"
        }
      }
    }
  ])

  lifecycle {
    ignore_changes = [container_definitions]
  }
}

resource "aws_ecs_service" "command_api" {
  name            = "ashwatch-command-api-service"
  cluster         = aws_ecs_cluster.ashwatch-cluster.id
  task_definition = aws_ecs_task_definition.ashwatch_command_api.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  lifecycle {
    ignore_changes = [task_definition]
  }

  network_configuration {
    subnets          = data.terraform_remote_state.network.outputs.public_subnet_ids
    security_groups  = [data.terraform_remote_state.network.outputs.fargate_sg_id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = data.terraform_remote_state.alb.outputs.command_target_group_arn
    container_name   = "command-api"
    container_port   = 8080
  }
}

resource "aws_ecs_service" "query_api" {
  name            = "ashwatch-query-api-service"
  cluster         = aws_ecs_cluster.ashwatch-cluster.id
  task_definition = aws_ecs_task_definition.ashwatch_query_api.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  lifecycle {
    ignore_changes = [task_definition]
  }

  network_configuration {
    subnets          = data.terraform_remote_state.network.outputs.public_subnet_ids
    security_groups  = [data.terraform_remote_state.network.outputs.fargate_sg_id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = data.terraform_remote_state.alb.outputs.query_target_group_arn
    container_name   = "query-api"
    container_port   = 8080
  }
}

data "aws_iam_policy_document" "command_api_permissions" {

  statement {
    effect    = "Allow"
    actions   = ["sns:Publish"]
    resources = [data.terraform_remote_state.sns_sqs.outputs.logging_topic_arn]
  }
}

data "aws_iam_policy_document" "query_api_permissions" {
  statement {
    effect  = "Allow"
    actions = ["dynamodb:GetItem", "dynamodb:Query"]
    resources = [
      data.terraform_remote_state.ecr_dynamodb.outputs.dynamodb_table_arn,
      "${data.terraform_remote_state.ecr_dynamodb.outputs.dynamodb_table_arn}/index/*"
    ]
  }
}

data "aws_iam_policy_document" "execution_secrets_access" {
  statement {
    effect    = "Allow"
    actions   = ["secretsmanager:GetSecretValue"]
    resources = [data.terraform_remote_state.db.outputs.db_secret_arn]
  }
}

resource "aws_iam_role_policy" "execution_secrets" {
  name   = "ashwatch-execution-secrets-policy"
  role   = aws_iam_role.ecs_execution.id
  policy = data.aws_iam_policy_document.execution_secrets_access.json
}

resource "aws_iam_role_policy" "command_api_policy" {
  name   = "ashwatch-command-api-policy"
  role   = aws_iam_role.command_api_task.id
  policy = data.aws_iam_policy_document.command_api_permissions.json
}

resource "aws_iam_role_policy" "query_api_policy" {
  name   = "ashwatch-query-api-policy"
  role   = aws_iam_role.query_api_task.id
  policy = data.aws_iam_policy_document.query_api_permissions.json
}

resource "aws_iam_role_policy_attachment" "execution_role_policy" {
  role       = aws_iam_role.ecs_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_cloudwatch_log_group" "command_api" {
  name              = "/ecs/ashwatch-command-api"
  retention_in_days = 7
}

resource "aws_cloudwatch_log_group" "query_api" {
  name              = "/ecs/ashwatch-query-api"
  retention_in_days = 7
}
