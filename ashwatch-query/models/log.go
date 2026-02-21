package models

type Log struct {
	tenantId  int
	projectId int
	userId    int
	ip        string
	timestamp string
	level     string
	message   string
	service   string
}
