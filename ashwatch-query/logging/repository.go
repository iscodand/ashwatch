package logging

import (
	"ashwatchquery/db"
	"context"

	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo"
)

type LogRepository interface {
	GetWithFilters(ctx context.Context, filter GetLogFilter) ([]Log, error)
	GetById(ctx context.Context, log *Log) Log
}

type logRepositoryImpl struct {
	client *mongo.Client
}

// GetById implements [LogRepository].
func (l *logRepositoryImpl) GetById(ctx context.Context, log *Log) Log {
	panic("unimplemented")
}

// GetWithFilters implements [LogRepository].
func (l *logRepositoryImpl) GetWithFilters(ctx context.Context, filter GetLogFilter) ([]Log, error) {
	client, err := db.Connect()
	if err != nil {
		return nil, err
	}
	defer client.Disconnect(ctx)

	var logs []Log
	collection := client.Database("ash_logging_dev").Collection("logs")
	filterDoc := bson.M{}

	if !filter.StartDate.IsZero() || !filter.EndDate.IsZero() {
		dateFilter := bson.M{}

		if !filter.StartDate.IsZero() {
			dateFilter["$gte"] = filter.StartDate.UnixNano()
		}
		if !filter.EndDate.IsZero() {
			dateFilter["$lte"] = filter.EndDate.UnixNano()
		}

		filterDoc["timestamp"] = dateFilter
	}

	cursor, err := collection.Find(ctx, filterDoc)
	if err != nil {
		return nil, err
	}
	defer cursor.Close(ctx)

	for cursor.Next(ctx) {
		var log Log
		if err := cursor.Decode(&log); err != nil {
			return nil, err
		}

		logs = append(logs, log)
	}

	return logs, nil
}

func NewLogRepository(client *mongo.Client) LogRepository {
	return &logRepositoryImpl{client: client}
}
