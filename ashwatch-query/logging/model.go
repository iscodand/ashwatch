package logging

type Log struct {
	ID        string `json:"id" dynamodbav:"id"`
	TenantID  string `json:"tenantId" dynamodbav:"tenantId"`
	ProjectID string `json:"projectId" dynamodbav:"projectId"`
	Author    string `json:"author" dynamodbav:"author"`
	Timestamp string `json:"timestamp" dynamodbav:"timestamp"`
	Level     string `json:"level" dynamodbav:"level"`
	Message   string `json:"message" dynamodbav:"message"`
}
