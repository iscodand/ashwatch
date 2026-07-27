package logging

import (
	"context"
	"fmt"
	"time"

	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"
)

const tenantTimestampIndex = "tenantId-timestamp-index"

type LogRepository interface {
	GetWithFilters(ctx context.Context, filter GetLogFilter) ([]Log, error)
	GetByID(ctx context.Context, id string) (*Log, error)
}

type logRepositoryImpl struct {
	client    *dynamodb.Client
	tableName string
}

func NewLogRepository(client *dynamodb.Client, tableName string) LogRepository {
	return &logRepositoryImpl{client: client, tableName: tableName}
}

func (r *logRepositoryImpl) GetByID(ctx context.Context, id string) (*Log, error) {
	out, err := r.client.GetItem(ctx, &dynamodb.GetItemInput{
		TableName: aws.String(r.tableName),
		Key: map[string]types.AttributeValue{
			"id": &types.AttributeValueMemberS{Value: id},
		},
	})
	if err != nil {
		return nil, fmt.Errorf("get item: %w", err)
	}
	if out.Item == nil {
		return nil, nil
	}

	var log Log
	if err := attributevalue.UnmarshalMap(out.Item, &log); err != nil {
		return nil, fmt.Errorf("unmarshal item: %w", err)
	}

	return &log, nil
}

func (r *logRepositoryImpl) GetWithFilters(ctx context.Context, filter GetLogFilter) ([]Log, error) {
	keyCond := expression.Key("tenantId").Equal(expression.Value(filter.TenantID))

	switch {
	case !filter.StartDate.IsZero() && !filter.EndDate.IsZero():
		keyCond = keyCond.And(expression.Key("timestamp").Between(
			expression.Value(filter.StartDate.Format(time.RFC3339Nano)),
			expression.Value(filter.EndDate.Format(time.RFC3339Nano)),
		))
	case !filter.StartDate.IsZero():
		keyCond = keyCond.And(expression.Key("timestamp").GreaterThanEqual(
			expression.Value(filter.StartDate.Format(time.RFC3339Nano)),
		))
	case !filter.EndDate.IsZero():
		keyCond = keyCond.And(expression.Key("timestamp").LessThanEqual(
			expression.Value(filter.EndDate.Format(time.RFC3339Nano)),
		))
	}

	expr, err := expression.NewBuilder().WithKeyCondition(keyCond).Build()
	if err != nil {
		return nil, fmt.Errorf("build expression: %w", err)
	}

	out, err := r.client.Query(ctx, &dynamodb.QueryInput{
		TableName:                 aws.String(r.tableName),
		IndexName:                 aws.String(tenantTimestampIndex),
		KeyConditionExpression:    expr.KeyCondition(),
		ExpressionAttributeNames:  expr.Names(),
		ExpressionAttributeValues: expr.Values(),
	})
	if err != nil {
		return nil, fmt.Errorf("query dynamodb: %w", err)
	}

	logs := make([]Log, 0, len(out.Items))
	if err := attributevalue.UnmarshalListOfMaps(out.Items, &logs); err != nil {
		return nil, fmt.Errorf("unmarshal items: %w", err)
	}

	return logs, nil
}
