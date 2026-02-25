namespace AshWatch.Application.Dtos;

public class GetLogsFilterRequest
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
