package logging

import "time"

type GetLogFilter struct {
	startDate time.Time
	endDate   time.Time
	keyword   string
}
