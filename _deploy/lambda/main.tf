terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
  }

  backend "s3" {
    bucket       = "ashwatch-tf-state-bucket"
    key          = "lambda/terraform.tfstate"
    region       = "us-east-1"
    use_lockfile = true
  }
}

provider "aws" {
  region = "us-east-1"
}

data "terraform_remote_state" "sns_sqs" {
  backend = "s3"
  config = {
    bucket = "ashwatch-tf-state-bucket"
    key    = "sns-sqs/terraform.tfstate"
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

data "aws_iam_policy_document" "lambda_assume_role" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "ashwatch-worker-lambda-role" {
  name               = "ashwatch-worker-lambda-role"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume_role.json
}

data "aws_iam_policy_document" "lambda_policy" {
  statement {
    sid    = "SQS"
    effect = "Allow"
    actions = [
      "sqs:ReceiveMessage",
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes"
    ]
    resources = [data.terraform_remote_state.sns_sqs.outputs.worker_queue_arn]
  }

  statement {
    sid    = "DynamoDB"
    effect = "Allow"
    actions = [
      "dynamodb:PutItem"
    ]
    resources = [data.terraform_remote_state.ecr_dynamodb.outputs.dynamodb_table_arn]
  }

  statement {
    sid    = "CloudWatchLogs"
    effect = "Allow"
    actions = [
      "logs:CreateLogGroup",
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = ["arn:aws:logs:*:*:*"]
  }
}

resource "aws_iam_role_policy" "ashwatch-worker-lambda-role-policy" {
  name   = "ashwatch-worker-lambda-policy"
  role   = aws_iam_role.ashwatch-worker-lambda-role.id
  policy = data.aws_iam_policy_document.lambda_policy.json
}

resource "aws_lambda_function" "ashwatch-worker" {
  function_name = "ashwatch-worker"
  role          = aws_iam_role.ashwatch-worker-lambda-role.arn
  package_type  = "Image"
  image_uri     = "${data.terraform_remote_state.ecr_dynamodb.outputs.worker_repository_url}:latest"
  timeout       = 30
  memory_size   = 128

  environment {
    variables = {
      DYNAMODB_TABLE = data.terraform_remote_state.ecr_dynamodb.outputs.dynamodb_table_name
    }
  }

  lifecycle {
    ignore_changes = [image_uri]
  }
}

resource "aws_lambda_event_source_mapping" "ashwatch-event-source-mapping" {
  function_name            = aws_lambda_function.ashwatch-worker.function_name
  event_source_arn         = data.terraform_remote_state.sns_sqs.outputs.worker_queue_arn
  batch_size               = 10
  function_response_types  = ["ReportBatchItemFailures"]
}
