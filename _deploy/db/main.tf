terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }

  backend "s3" {
    bucket       = "ashwatch-tf-state-bucket"
    key          = "db/terraform.tfstate"
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

resource "aws_db_subnet_group" "db_subnet_group" {
  subnet_ids = data.terraform_remote_state.network.outputs.private_subnet_ids
}

resource "random_password" "db" {
  length           = 16
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

resource "aws_secretsmanager_secret" "ashwatch_db_secret" {
  name = "ashwatch_db_secret"
}

resource "aws_secretsmanager_secret_version" "ashwatch_secret_version" {
  secret_id = aws_secretsmanager_secret.ashwatch_db_secret.id
  secret_string = jsonencode({
    username = "admin_bitch"
    password = "${random_password.db.result}"
  })
}

resource "aws_db_instance" "ashwatch_db" {
  identifier             = "ashwatchdb"
  db_name                = "ashwatch_db"
  engine                 = "postgres"
  engine_version         = "18"
  instance_class         = "db.t3.micro"
  allocated_storage      = 20
  db_subnet_group_name   = aws_db_subnet_group.db_subnet_group.name
  vpc_security_group_ids = [data.terraform_remote_state.network.outputs.aurora_sg_id]
  username               = "admin_bitch"
  password               = random_password.db.result
  skip_final_snapshot    = true
  publicly_accessible    = false
}
