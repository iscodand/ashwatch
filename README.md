# AshWatch

A structured logging platform built as a learning project to explore different technologies working together in a polyglot, event-driven architecture.

## Architecture

AshWatch follows a **CQRS (Command Query Responsibility Segregation)** pattern where writes and reads are handled by completely separate services, written in different languages.

### Services

| Service | Language | Role |
|---------|----------|------|
| **ashwatch-api** | C# / .NET 10 | Command API -- receives log entries, manages tenants and projects (PostgreSQL via EF Core), and publishes log events to AWS SNS and Kafka |
| **ashwatch-worker** | Python 3.11 | Consumer -- AWS Lambda triggered by SQS, deserializes each log event and persists it into DynamoDB |
| **ashwatch-query** | Go 1.25 | Query API -- reads logs from DynamoDB and serves them with filtering support |

### Data Flow

1. A client sends a log entry (single or batch) to the **Command API** (`POST /logs`)
2. The API validates the payload, publishes it to **AWS SNS** (FIFO topic), and produces it to **Kafka**
3. SNS fans out the message to an **AWS SQS** FIFO queue
4. The **Worker** (AWS Lambda) is triggered in batches by the SQS event source mapping, deserializes each message, and inserts it into **DynamoDB**
5. The **Query API** reads from DynamoDB and returns logs to the client (`GET /logs`)

## Tech Stack

- **C# / .NET 10** -- ASP.NET Core Web API, Entity Framework Core, NSwag (OpenAPI/Swagger)
- **Python 3.11** -- boto3, deployed as an AWS Lambda function (SQS-triggered)
- **Go 1.25** -- Chi router, AWS SDK for Go (DynamoDB)
- **PostgreSQL 16** -- Tenant and project metadata (relational data)
- **DynamoDB** -- Log event storage (key-value/document store)
- **AWS SNS + SQS (FIFO)** -- Async message delivery with ordering guarantees
- **AWS Lambda** -- Worker runtime, packaged as a container image and triggered by the SQS event source mapping
- **Apache Kafka** -- Secondary event stream
- **Docker** -- Containerized services and local infrastructure

## API Endpoints

### Command API (ashwatch-api) -- `:8080`

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/logs` | Create a single log entry |
| `POST` | `/logs/batch` | Create multiple log entries |
| `POST` | `/tenants` | Create a tenant |
| `GET` | `/tenants` | List all tenants |
| `GET` | `/tenants/{id}` | Get tenant by ID |
| `PUT` | `/tenants/{id}` | Update a tenant |
| `DELETE` | `/tenants/{id}` | Delete a tenant |
| `POST` | `/projects` | Create a project |
| `GET` | `/projects` | List projects (optional `?tenantId=`) |
| `GET` | `/projects/{id}` | Get project by ID |
| `PUT` | `/projects/{id}` | Update a project |
| `DELETE` | `/projects/{id}` | Delete a project |

Swagger UI is available at `/swagger` when running in development.

### Query API (ashwatch-query) -- `:8080`

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/logs` | Get logs with optional filters (`startDate`, `endDate` as RFC 3339) |
| `GET` | `/health` | Health check |

## Log Levels

`TRACE` | `DEBUG` | `INFO` | `WARN` | `ERROR` | `FATAL`

## Log Entry Schema

```json
{
  "tenantId": "uuid",
  "projectId": "uuid",
  "author": "string",
  "message": "string (max 4000 chars)",
  "level": "INFO",
  "timestamp": "2026-01-01T00:00:00Z"
}
```

## Getting Started

### Prerequisites

- Docker and Docker Compose
- AWS credentials configured (`~/.aws/credentials`) with access to SNS and SQS
- .NET 10 SDK, Python 3.11+, Go 1.25+

### Infrastructure

Start the local databases and tools:

```bash
docker compose up -d
```

This starts PostgreSQL, pgAdmin, MongoDB, Mongo Express, and Kafka.

| Service | URL |
|---------|-----|
| PostgreSQL | `localhost:5432` |
| pgAdmin | `localhost:5050` |
| MongoDB | `localhost:27017` |
| Mongo Express | `localhost:8081` |
| Kafka | `localhost:9092` |

### Running the Services

**Command API:**

```bash
cd ashwatch-api/src/AshWatch.Api
dotnet run
```

**Worker:**

The worker runs as an AWS Lambda function (container image) triggered by the SQS event source mapping -- see `_deploy/lambda`. To run it locally for testing:

```bash
cd ashwatch-worker
pip install -r requirements.txt
AWS_REGION=us-east-1 DYNAMODB_TABLE=ashwatch python -c "
from main import lambda_handler
lambda_handler({'Records': [...]}, None)
"
```

**Query API:**

```bash
cd ashwatch-query
cp .env.example .env  # fill in MONGO_URI
go run main.go
```

### Environment Variables

Each service has a `.env.example` with the required variables. Copy it to `.env` and fill in the values.

## License

[MIT](LICENSE)
