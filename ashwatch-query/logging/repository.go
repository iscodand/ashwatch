package logging

import (
	"context"
	"database/sql"
)

type LogRepository interface {
	GetWithFilters(ctx context.Context, log *Log) []Log
	GetById(ctx context.Context, log *Log) Log
}

type logRepositoryImpl struct {
	db *sql.DB
}

// GetById implements [LogRepository].
func (l *logRepositoryImpl) GetById(ctx context.Context, log *Log) Log {
	panic("unimplemented")
}

// GetWithFilters implements [LogRepository].
func (l *logRepositoryImpl) GetWithFilters(ctx context.Context, log *Log) []Log {
	panic("unimplemented")
}

func NewLogRepository(db *sql.DB) LogRepository {
	return &logRepositoryImpl{db: db}
}
