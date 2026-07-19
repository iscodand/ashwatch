terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
  }

  backend "s3" {
    bucket       = "ashwatch-tf-state-bucket"
    key          = "ci-cd/terraform.tfstate"
    region       = "us-east-1"
    use_lockfile = true
  }
}

provider "aws" {
  region = "us-east-1"
}

resource "aws_iam_openid_connect_provider" "github" {
  url             = "https://token.actions.githubusercontent.com"
  client_id_list  = ["sts.amazonaws.com"]
  thumbprint_list = ["6938fd4d98bab03faadb97b34396831e3780aea1"]
}

data "aws_iam_policy_document" "github_assume_role" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRoleWithWebIdentity"]

    principals {
      type        = "Federated"
      identifiers = [aws_iam_openid_connect_provider.github.arn]
    }

    condition {
      test     = "StringEquals"
      variable = "token.actions.githubusercontent.com:aud"
      values   = ["sts.amazonaws.com"]
    }

    condition {
      test     = "StringLike"
      variable = "token.actions.githubusercontent.com:sub"
      values   = ["repo:iscodand/ashwatch:*"]
    }
  }
}

resource "aws_iam_role" "github_actions" {
  name               = "ashwatch-github-actions"
  assume_role_policy = data.aws_iam_policy_document.github_assume_role.json
}

data "aws_iam_policy_document" "github_ecr_push" {
  statement {
    sid    = "ECRAuth"
    effect = "Allow"
    actions = [
      "ecr:GetAuthorizationToken"
    ]
    resources = ["*"]
  }

  statement {
    sid    = "ECRPush"
    effect = "Allow"
    actions = [
      "ecr:BatchCheckLayerAvailability",
      "ecr:PutImage",
      "ecr:InitiateLayerUpload",
      "ecr:UploadLayerPart",
      "ecr:CompleteLayerUpload",
      "ecr:BatchGetImage"
    ]
    resources = [
      data.terraform_remote_state.ecr_dynamodb.outputs.command_api_repository_arn,
      data.terraform_remote_state.ecr_dynamodb.outputs.query_api_repository_arn,
      data.terraform_remote_state.ecr_dynamodb.outputs.worker_repository_arn,
    ]
  }

  statement {
    sid    = "LambdaDeploy"
    effect = "Allow"
    actions = [
      "lambda:UpdateFunctionCode"
    ]
    resources = ["arn:aws:lambda:us-east-1:*:function:ashwatch-worker"]
  }
}

resource "aws_iam_role_policy" "github_runner_ecr" {
  name   = "ashwatch-github-runner-ecr-policy"
  role   = aws_iam_role.github_actions.id
  policy = data.aws_iam_policy_document.github_ecr_push.json
}

data "terraform_remote_state" "ecr_dynamodb" {
  backend = "s3"
  config = {
    bucket = "ashwatch-tf-state-bucket"
    key    = "ecr-dynamodb/terraform.tfstate"
    region = "us-east-1"
  }
}
