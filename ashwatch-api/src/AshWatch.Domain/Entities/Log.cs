namespace AshWatch.Domain.Entities;

public class Log
{
    public string Id { get; set; }
    public int TenantId { get; set; }
    public int ProjectId { get; set; }
    public string Author { get; set; }
    public string Message { get; set; }
    public string Level { get; set; }
    public DateTime Timestamp { get; set; }
}