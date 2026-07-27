package logging

import "time"

type GetLogFilter struct {
	TenantID  string
	StartDate time.Time
	EndDate   time.Time
}
