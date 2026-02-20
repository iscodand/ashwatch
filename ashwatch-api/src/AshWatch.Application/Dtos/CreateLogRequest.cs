namespace AshWatch.Application.Dtos;

public class CreateLogRequest
{
    public int TenantId { get; set; }
    public int ProjectId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
