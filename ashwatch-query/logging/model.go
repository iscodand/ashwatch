package logging

import "time"

type Log struct {
	_id       int
	TenantId  int
	ProjectId int
	Author    string
	Timestamp time.Time
	Level     string
	Message   string
}
