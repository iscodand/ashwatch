terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
  }

  backend "s3" {
    bucket       = "ashwatch-tf-state-bucket"
    key          = "ecr-dynamodb/terraform.tfstate"
    region       = "us-east-1"
    use_lockfile = true
  }
}

provider "aws" {
  region = "us-east-1"
}

resource "aws_ecr_repository" "ashwatch-command-api" {
  name                 = "ashwatch-command-api"
  image_tag_mutability = "MUTABLE_WITH_EXCLUSION"

  image_scanning_configuration {
    scan_on_push = true
  }

  image_tag_mutability_exclusion_filter {
    filter      = "latest*"
    filter_type = "WILDCARD"
  }

  tags = {
    Name = "ashwatch-command-api"
  }
}

resource "aws_ecr_repository" "ashwatch-query-api" {
  name                 = "ashwatch-query-api"
  image_tag_mutability = "MUTABLE_WITH_EXCLUSION"

  image_scanning_configuration {
    scan_on_push = true
  }

  image_tag_mutability_exclusion_filter {
    filter      = "latest*"
    filter_type = "WILDCARD"
  }

  tags = {
    Name = "ashwatch-query-api"
  }
}

resource "aws_ecr_repository" "ashwatch-worker" {
  name                 = "ashwatch-worker"
  image_tag_mutability = "MUTABLE_WITH_EXCLUSION"

  image_scanning_configuration {
    scan_on_push = true
  }

  image_tag_mutability_exclusion_filter {
    filter      = "latest*"
    filter_type = "WILDCARD"
  }

  tags = {
    Name = "ashwatch-worker"
  }
}

resource "aws_dynamodb_table" "ashwatch" {
  name         = "ashwatch"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "id"

  attribute {
    name = "id"
    type = "S"
  }

  attribute {
    name = "tenantId"
    type = "S"
  }

  attribute {
    name = "timestamp"
    type = "S"
  }

  global_secondary_index {
    name            = "tenantId-timestamp-index"
    hash_key        = "tenantId"
    range_key       = "timestamp"
    projection_type = "ALL"
  }

  tags = {
    Name = "ashwatch-dynamodb"
  }
}
