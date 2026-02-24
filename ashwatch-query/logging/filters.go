package logging

import "time"

type GetLogFilter struct {
	StartDate time.Time
	EndDate   time.Time
	Keyword   string
}
