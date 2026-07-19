terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
  }

  backend "s3" {
    bucket       = "ashwatch-tf-state-bucket"
    key          = "sns-sqs/terraform.tfstate"
    region       = "us-east-1"
    use_lockfile = true
  }
}

provider "aws" {
  region = "us-east-1"
}

resource "aws_sns_topic" "ashwatch-logging-sns" {
  name = "ashwatch-logging-sns"

  tags = {
    Name = "ashwatch-logging-sns"
  }
}

resource "aws_sqs_queue" "ashwatch-sqs" {
  name                       = "ashwatch-sqs"
  visibility_timeout_seconds = 30

  tags = {
    Name = "ashwatch-sqs"
  }
}

resource "aws_sns_topic_subscription" "ashwatch_topic_subscription" {
  protocol  = "sqs"
  endpoint  = aws_sqs_queue.ashwatch-sqs.arn
  topic_arn = aws_sns_topic.ashwatch-logging-sns.arn
}

data "aws_iam_policy_document" "sqs_policy" {
  statement {
    effect  = "Allow"
    actions = ["sqs:SendMessage"]

    principals {
      type        = "Service"
      identifiers = ["sns.amazonaws.com"]
    }

    resources = [aws_sqs_queue.ashwatch-sqs.arn]

    condition {
      test     = "ArnEquals"
      variable = "aws:SourceArn"
      values   = [aws_sns_topic.ashwatch-logging-sns.arn]
    }
  }
}

resource "aws_sqs_queue_policy" "ashwatch_queue_policy" {
  queue_url = aws_sqs_queue.ashwatch-sqs.id
  policy    = data.aws_iam_policy_document.sqs_policy.json
}
