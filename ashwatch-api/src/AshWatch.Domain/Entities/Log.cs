namespace AshWatch.Domain.Entities;

public class Log
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public string Author { get; set; }
    public string Message { get; set; }
    public string Level { get; set; }
    public DateTime Timestamp { get; set; }
}
