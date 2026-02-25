namespace AshWatch.Application.Dtos;

public class CreateLogRequest
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
